using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

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

        public ResultadoOperacao FinalizarVenda(VendaModel venda, int? usuarioId)
        {
            if (venda == null || venda.Itens.Count == 0)
            {
                return ResultadoOperacao.Erro("Adicione pelo menos um item à venda.");
            }

            if (string.IsNullOrEmpty(venda.VendaTipoPagamento))
            {
                return ResultadoOperacao.Erro("Selecione um tipo de pagamento.");
            }
            _unitOfWork.BeginTransaction();

            try
            {
                var ids = venda.Itens.Select(i => i.ProdutoID).ToList();
                var produtos = _produtoRepository.ObterPorIds(ids);

                decimal totalVenda = 0;

                foreach (var item in venda.Itens)
                {
                    var produto = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID);

                    if (produto == null)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro($"Produto não encontrado: ID {item.ProdutoID}");
                    }

                    if ((produto.ProdutoQtdEstoque ?? 0) < item.ItemVendaQtd)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao.Erro($"Estoque insuficiente para: {produto.ProdutoNome}");
                    }

                    decimal precoBase = produto.ProdutoPrecoVenda ?? 0;
                    decimal desconto = produto.ProdutoPromocao;
                    decimal precoFinal = desconto > 0
                        ? precoBase - (precoBase * (desconto / 100))
                        : precoBase;

                    item.ItemVendaPreco = precoFinal;
                    item.ItemVendaTotal = precoFinal * item.ItemVendaQtd;
                    totalVenda += item.ItemVendaTotal;
                }
                venda.VendaValorTotal = totalVenda;
                venda.UsuarioID = usuarioId;

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