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
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VendaService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            IUnitOfWork unitOfWork)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _unitOfWork = unitOfWork;
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

                    decimal precoBase = produto.ProdutoPrecoVenda ?? 0;
                    decimal desconto = produto.ProdutoPromocao;
                    decimal precoFinal = desconto > 0
                        ? precoBase - (precoBase * (desconto / 100))
                        : precoBase;

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
                    var produto = produtos.First(p => p.ProdutoID == item.ProdutoID);
                    produto.ProdutoQtdEstoque -= item.ItemVendaQtd;

                    var movimentacao = new MovimentacaoEstoqueModel
                    {
                        ProdutoID = produto.ProdutoID,
                        MovimentacaoQtd = item.ItemVendaQtd,
                        MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Saida,
                        MovimentacaoData = DateTime.Now,
                        ItemVendaID = item.ItemVendaID,
                        UsuarioID = usuarioId
                    };

                    _movimentacaoRepository.Adicionar(movimentacao);
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

            var ids = venda.Itens.Select(i => i.ProdutoID).ToList();
            var produtos = _produtoRepository.ObterPorIds(ids);

            foreach (var item in venda.Itens)
            {
                var produto = produtos.First(p => p.ProdutoID == item.ProdutoID);

                produto.ProdutoQtdEstoque += item.ItemVendaQtd;

                var movimentacao = new MovimentacaoEstoqueModel
                {
                    ProdutoID = produto.ProdutoID,
                    MovimentacaoQtd = item.ItemVendaQtd,
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Cancelamento,
                    MovimentacaoData = DateTime.Now,
                    ItemVendaID = item.ItemVendaID,
                    MovimentacaoDescricao = descricao,
                    UsuarioID = usuarioId,
                };

                _movimentacaoRepository.Adicionar(movimentacao);
            }

            venda.VendaCancelada = true;
            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Venda cancelada com sucesso!");
        }
    }
}