using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;
using PolarDrinks.Models.Requests;

namespace PolarDrinks.Services
{
    public class VendaService : IVendaService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovimentacaoEstoqueService _movimentacaoEstoqueService;

        public VendaService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork,
            IMovimentacaoEstoqueService movimentacaoEstoqueService)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
            _movimentacaoEstoqueService = movimentacaoEstoqueService;
        }

        public List<VendaModel> ListarVendas(DateTime? dataInicio, DateTime? dataFim)
        {
            return _vendaRepository.ObterPorPeriodo(dataInicio, dataFim);
        }

        public List<ProdutoModel> ListarProdutosAtivos()
        {
            return _produtoRepository.ObterAtivos();
        }

        public VendaModel? ObterVendaParaCancelamento(int id)
        {
            return _vendaRepository.ObterPorId(id);
        }

        public VendaDetalhesViewModel? ObterDetalhesVenda(int id)
        {
            var venda = _vendaRepository.ObterPorId(id);
            if (venda == null)
                return null;

            var motivo = _vendaRepository.ObterMotivoCancelamento(id);

            return new VendaDetalhesViewModel
            {
                Venda = venda,
                MotivoCancelamento = motivo?.Descricao,
                UsuarioCancelamento = motivo?.UsuarioNome
            };
        }

        public ResultadoOperacao FinalizarVenda(FinalizarVendaRequest request, int? usuarioId)
        {
            if (request == null || request.Itens == null || request.Itens.Count == 0)
            {
                return ResultadoOperacao.Erro("Adicione pelo menos um item à venda.");
            }

            if (string.IsNullOrWhiteSpace(request.VendaTipoPagamento))
            {
                return ResultadoOperacao.Erro("Selecione um tipo de pagamento.");
            }

            if (!VendaModel.TipoPagamento.EhValido(request.VendaTipoPagamento))
            {
                return ResultadoOperacao.Erro("Tipo de pagamento inválido.");
            }

            if (request.Itens.Any(i => i.ItemVendaQtd <= 0))
            {
                return ResultadoOperacao.Erro("A quantidade de cada item deve ser maior que zero.");
            }

            _unitOfWork.BeginTransaction();

            try
            {
                var ids = request.Itens.Select(i => i.ProdutoID).Distinct().ToList();
                var produtos = _produtoRepository.ObterPorIds(ids);

                var quantidadePorProduto = request.Itens
                    .GroupBy(i => i.ProdutoID)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.ItemVendaQtd));

                foreach (var (produtoId, quantidadeTotal) in quantidadePorProduto)
                {
                    var produto = produtos.FirstOrDefault(p => p.ProdutoID == produtoId);

                    if (produto == null)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro($"Produto não encontrado: ID {produtoId}");
                    }

                    if (!produto.ProdutoAtivo)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro($"Produto inativo: {produto.ProdutoNome}");
                    }

                    if ((produto.ProdutoQtdEstoque ?? 0) < quantidadeTotal)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro($"Estoque insuficiente para: {produto.ProdutoNome}");
                    }
                }

                var venda = new VendaModel
                {
                    VendaTipoPagamento = request.VendaTipoPagamento!, 
                    VendaData = DateTime.Now,
                    VendaCancelada = false,
                    UsuarioID = usuarioId
                };

                decimal totalVenda = 0;

                foreach (var itemRequest in request.Itens)
                {
                    var produto = produtos.First(p => p.ProdutoID == itemRequest.ProdutoID);

                    decimal precoFinal = produto.CalcularPrecoFinal();

                    venda.Itens.Add(new ItemVendaModel
                    {
                        ProdutoID = produto.ProdutoID,
                        ItemVendaQtd = itemRequest.ItemVendaQtd,
                        ItemVendaPreco = precoFinal,
                        ItemVendaCusto = produto.ProdutoPrecoCusto ?? 0
                    });

                    totalVenda += itemRequest.ItemVendaQtd * precoFinal;
                }

                venda.VendaValorTotal = totalVenda;

                _vendaRepository.Adicionar(venda);
                _unitOfWork.SaveChanges();

                foreach (var item in venda.Itens)
                {
                    var saida = _movimentacaoEstoqueService.RegistrarSaida(
                        item.ProdutoID,
                        item.ItemVendaQtd,
                        MovimentacaoEstoqueModel.Tipos.Saida,
                        usuarioId,
                        itemVendaId: item.ItemVendaID);

                    if (!saida.Sucesso)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro(saida.Mensagem!);
                    }
                }

                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();

                return ResultadoOperacao.Ok("Venda realizada com sucesso!");
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return ResultadoOperacao.Erro($"Erro ao salvar venda: {ex.Message}");
            }
        }
        public ResultadoOperacao CancelarVenda(int id, string? descricao, int? usuarioId)
        {
            var venda = _vendaRepository.ObterPorId(id);

            if (venda == null)
            {
                return ResultadoOperacao.Erro("Venda não encontrada.");
            }

            if (DateTime.Now > venda.VendaData.AddHours(24))
            {
                return ResultadoOperacao.Erro("Não é possível cancelar a venda após 24 horas.");
            }

            if (venda.VendaCancelada)
            {
                return ResultadoOperacao.Erro("Venda já está cancelada.");
            }

            foreach (var item in venda.Itens)
            {
                var devolucao = _movimentacaoEstoqueService.RegistrarDevolucao(
                    item.ProdutoID,
                    item.ItemVendaQtd,
                    MovimentacaoEstoqueModel.Tipos.Cancelamento,
                    usuarioId,
                    descricao: descricao,
                    itemVendaId: item.ItemVendaID);

                if (!devolucao.Sucesso)
                {
                    _unitOfWork.Rollback();
                    return ResultadoOperacao.Erro(devolucao.Mensagem!);
                }
            }

            venda.VendaCancelada = true;
            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Venda cancelada com sucesso!");
        }
    }
}