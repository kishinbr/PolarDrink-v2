using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Modules.Compras.Models;
using PolarDrinks.Modules.Compras.Repositories;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;

namespace PolarDrinks.Modules.Compras.Services
{
    public class CompraEstoqueService : ICompraEstoqueService
    {
        private readonly ICompraEstoqueRepository _compraRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IMovimentacaoEstoqueService _movimentacaoEstoqueService;
        private readonly IUnitOfWork _unitOfWork;
        public CompraEstoqueService(
            ICompraEstoqueRepository compraRepository,
            IProdutoRepository produtoRepository,
            IFornecedorRepository fornecedorRepository,
            IUnitOfWork unitOfWork,
            IMovimentacaoEstoqueService movimentacaoEstoqueService)
        {
            _compraRepository = compraRepository;
            _produtoRepository = produtoRepository;
            _fornecedorRepository = fornecedorRepository;
            _unitOfWork = unitOfWork;
            _movimentacaoEstoqueService = movimentacaoEstoqueService;
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

        public ResultadoOperacao CadastrarCompra(
    int? fornecedorId, IEnumerable<(int? ProdutoId, int Quantidade, decimal Preco)> itens, int? usuarioId)
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
                if (item.ProdutoId.HasValue && item.Quantidade > 0 && item.Preco > 0)
                {
                    compra.Itens.Add(new ItemCompraModel
                    {
                        ProdutoID = item.ProdutoId.Value,
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

        public (CompraEstoqueModel Compra, string? UsuarioConfirmacao)? ObterDetalhes(int id)
        {
            var compra = _compraRepository.ObterDetalhes(id);
            if (compra == null)
                return null;

            var usuarioConfirmacao = _compraRepository.ObterUsuarioConfirmacaoEntrada(id);

            return (compra, usuarioConfirmacao);
        }

        public CompraEstoqueModel? ObterParaExcluir(int id)
        {
            var compra = _compraRepository.ObterDetalhes(id);
            if (compra == null || compra.CompraStatus == "Concluído")
                return null;

            return compra;
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

            foreach (var item in compra.Itens)
            {
                var entrada = _movimentacaoEstoqueService.RegistrarEntrada(
                    item.ProdutoID,
                    item.ItemCompraQtd,
                    usuarioId,
                    item.ItemCompraID);

                if (!entrada.Sucesso)
                {
                    _unitOfWork.Rollback();
                    return ResultadoOperacao.Erro(entrada.Mensagem!);
                }
            }

            compra.CompraStatus = "Concluído";
            compra.CompraDataEntrega = DateTime.Now;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Entrega confirmada e estoque atualizado!");
        }

        public ResultadoOperacao ConfirmarExclusao(int id)
        {
            var compra = _compraRepository.ObterParaConfirmarEntrega(id);

            if (compra == null)
            {
                return ResultadoOperacao.Erro("Compra não encontrada.");
            }
            if (compra.CompraStatus == "Concluído")  
                return ResultadoOperacao.Erro("Não é possível excluir uma compra já concluída.");

            _compraRepository.Remover(compra);
            _compraRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Compra excluída com sucesso!");
        }

    }
}