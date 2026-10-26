using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;
using PolarDrinks.ViewModels;

namespace PolarDrinks.Services
{
    public class CompraEstoqueService : ICompraEstoqueService
    {
        private readonly ICompraEstoqueRepository _compraRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;

        public CompraEstoqueService(
            ICompraEstoqueRepository compraRepository,
            IProdutoRepository produtoRepository,
            IFornecedorRepository fornecedorRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository)
        {
            _compraRepository = compraRepository;
            _produtoRepository = produtoRepository;
            _fornecedorRepository = fornecedorRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        public (List<CompraEstoqueModel> Pendentes, List<CompraEstoqueModel> Concluidas) ListarComprasPorStatus()
        {
            var pendentes = _compraRepository.ObterPorStatus("Aguardando");
            var concluidas = _compraRepository.ObterPorStatus("Concluído");
            return (pendentes, concluidas);
        }

        public List<FornecedorModel> ListarFornecedoresAtivos()
        {
            return _fornecedorRepository.ObterAtivos();
        }

        public List<ProdutoModel> ListarTodosProdutos()
        {
            return _produtoRepository.ObterTodos();
        }

        public List<ProdutoModel> ListarProdutosAtivosOrdenados()
        {
            return _produtoRepository.ObterAtivos()
                .OrderBy(p => p.ProdutoNome)
                .ToList();
        }

        public void PreencherNomesProdutos(List<ItemCompraCreateVM> itens)
        {
            var ids = itens.Where(i => i.ProdutoID.HasValue).Select(i => i.ProdutoID!.Value).ToList();
            var produtos = _produtoRepository.ObterPorIds(ids);

            foreach (var item in itens)
            {
                if (item.ProdutoID.HasValue)
                {
                    item.ProdutoNome = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID)?.ProdutoNome;
                }
            }
        }
        public ResultadoOperacao CadastrarCompra(int? fornecedorId, List<ItemCompraCreateVM> itens, int? usuarioId)
        {
            var compra = new CompraEstoqueModel
            {
                FornecedorID = fornecedorId!.Value,
                CompraData = DateTime.Now,
                CompraStatus = "Aguardando",
                UsuarioID = usuarioId,
                Itens = new List<ItemCompraModel>()
            };

            foreach (var item in itens)
            {
                if (item.ProdutoID.HasValue && item.Quantidade > 0 && item.Preco > 0)
                {
                    compra.Itens.Add(new ItemCompraModel
                    {
                        ProdutoID = item.ProdutoID.Value,
                        ItemCompraQtd = item.Quantidade,
                        ItemCompraPreco = item.Preco
                    });
                }
            }

            if (compra.Itens.Count == 0)
            {
                return ResultadoOperacao.Erro("Adicione pelo menos um produto", campoErro: "");
            }

            _compraRepository.Adicionar(compra);
            _compraRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Compra cadastrada com sucesso!");
        }

        public CompraDetalhesViewModel? ObterDetalhes(int id, bool podeConfirmar)
        {
            var compra = _compraRepository.ObterDetalhes(id);
            if (compra == null)
                return null;

            var usuarioConfirmacao = _compraRepository.ObterUsuarioConfirmacaoEntrada(id);

            return new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = podeConfirmar,
                UsuarioConfirmacao = usuarioConfirmacao
            };
        }

        public CompraDetalhesViewModel? ObterParaExcluir(int id)
        {
            var compra = _compraRepository.ObterDetalhes(id);
            if (compra == null)
                return null;

            return new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = false
            };
        }
        public ResultadoOperacao ConfirmarEntrega(int id, int? usuarioId)
        {
            var compra = _compraRepository.ObterParaConfirmarEntrega(id);

            if (compra == null)
            {
                return ResultadoOperacao.Erro("Compra não encontrada.");
            }

            if (compra.CompraStatus == "Concluído")
            {
                return ResultadoOperacao.Ok();
            }

            var ids = compra.Itens.Select(i => i.ProdutoID).ToList();
            var produtos = _produtoRepository.ObterPorIds(ids);

            foreach (var item in compra.Itens)
            {
                var produto = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID);

                if (produto == null) continue;

                produto.ProdutoQtdEstoque += item.ItemCompraQtd;

                var movimentacao = new MovimentacaoEstoqueModel
                {
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Entrada,
                    MovimentacaoQtd = item.ItemCompraQtd,
                    ProdutoID = produto.ProdutoID,
                    ItemCompraID = item.ItemCompraID,
                    MovimentacaoData = DateTime.Now,
                    UsuarioID = usuarioId
                };

                _movimentacaoRepository.Adicionar(movimentacao);
            }

            compra.CompraStatus = "Concluído";
            compra.CompraDataEntrega = DateTime.Now;
            _compraRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Entrega confirmada e estoque atualizado!");
        }

        public ResultadoOperacao ConfirmarExclusao(int id)
        {
            var compra = _compraRepository.ObterParaConfirmarEntrega(id);

            if (compra == null)
            {
                return ResultadoOperacao.Erro("Compra não encontrada.");
            }

            _compraRepository.Remover(compra);
            _compraRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Compra excluída com sucesso!");
        }

    }
}