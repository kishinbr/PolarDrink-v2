using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;
using Microsoft.AspNetCore.Http;
using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly IArmazenamentoService _armazenamentoService;

        public EstoqueService(
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            IArmazenamentoService armazenamentoService)
        {
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _armazenamentoService = armazenamentoService;
        }

        public List<ProdutoModel> ListarProdutos()
        {
            return _produtoRepository.ObterTodos();
        }

        public List<ProdutoModel> ListarProdutosAtivos()
        {
            return _produtoRepository.ObterAtivos();
        }

        public ProdutoModel? ObterProduto(int id)
        {
            return _produtoRepository.ObterPorId(id);
        }
        public List<int> ObterCategoriasDoProduto(int produtoId)
        {
            return _produtoRepository.ObterCategoriasDoProduto(produtoId);
        }

        public List<MovimentacaoEstoqueModel> ObterMovimentacoes(int produtoId)
        {
            return _movimentacaoRepository.ObterPorProduto(produtoId);
        }


        public ResultadoOperacao CadastrarProduto(ProdutoModel produto, List<int> categoriaIds, IFormFile? imagem)
        {
            if (_produtoRepository.ExisteCodigoBarra(produto.ProdutoCodBarra!))
            {
                return ResultadoOperacao.Erro(
                    "Este código de barras já está cadastrado.",
                    campoErro: nameof(ProdutoModel.ProdutoCodBarra));
            }

            _produtoRepository.Adicionar(produto);
            _produtoRepository.SalvarAlteracoes(); // salva primeiro para gerar o ProdutoID

            if (imagem != null && imagem.Length > 0)
            {
                produto.ProdutoImagemUrl = _armazenamentoService.SalvarImagemProduto(imagem, produto.ProdutoID);
            }

            _produtoRepository.DefinirCategoriasDoProduto(produto.ProdutoID, categoriaIds);
            _produtoRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Produto cadastrado com sucesso!");
        }
        public ResultadoOperacao EditarProduto(ProdutoModel produto, List<int> categoriaIds, IFormFile? imagem)
        {
            if (_produtoRepository.ExisteCodigoBarra(produto.ProdutoCodBarra!, produto.ProdutoID))
            {
                return ResultadoOperacao.Erro(
                    "Este código de barras já está cadastrado.",
                    campoErro: nameof(ProdutoModel.ProdutoCodBarra));
            }

            var produtoDb = _produtoRepository.ObterPorId(produto.ProdutoID);
            if (produtoDb == null)
            {
                return ResultadoOperacao.Erro("Produto não encontrado.");
            }

            produtoDb.ProdutoNome = produto.ProdutoNome;
            produtoDb.ProdutoDescricao = produto.ProdutoDescricao;
            produtoDb.ProdutoCodBarra = produto.ProdutoCodBarra;
            produtoDb.ProdutoPrecoVenda = produto.ProdutoPrecoVenda;
            produtoDb.ProdutoAtivo = produto.ProdutoAtivo;
            produtoDb.ProdutoEstoqueMinimo = produto.ProdutoEstoqueMinimo;
            produtoDb.ProdutoPrecoCusto = produto.ProdutoPrecoCusto;
            produtoDb.ProdutoPromocao = produto.ProdutoPromocao;
            produtoDb.ProdutoQtdEstoque = produto.ProdutoQtdEstoque;

            if (imagem != null && imagem.Length > 0)
            {
                _armazenamentoService.RemoverImagemProduto(produtoDb.ProdutoImagemUrl);
                produtoDb.ProdutoImagemUrl = _armazenamentoService.SalvarImagemProduto(imagem, produtoDb.ProdutoID);
            }
            // se nenhuma imagem nova foi enviada, produtoDb.ProdutoImagemUrl simplesmente não é tocado, mantendo a atual

            _produtoRepository.DefinirCategoriasDoProduto(produtoDb.ProdutoID, categoriaIds);
            _produtoRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Produto atualizado com sucesso!");
        }
        public ResultadoOperacao EdicaoRapida(int produtoId, decimal? precoVenda, decimal? promocao)
        {
            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null)
            {
                return ResultadoOperacao.Erro("Produto não encontrado!");
            }

            if (precoVenda == null || precoVenda < 0)
            {
                return ResultadoOperacao.Erro("Preço inválido!");
            }

            if (promocao == null || promocao > 100 || promocao < 0)
            {
                return ResultadoOperacao.Erro("Promoção inválida!");
            }

            produto.ProdutoPrecoVenda = precoVenda.Value;
            produto.ProdutoPromocao = promocao ?? 0;

            _produtoRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Produto atualizado com sucesso!");
        }
        public ResultadoOperacao AjustarEstoque(int produtoId, int novaQuantidade, string descricao, int? usuarioId)
        {
            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null)
            {
                return ResultadoOperacao.Erro("Produto não encontrado.");
            }

            int quantidadeAntiga = produto.ProdutoQtdEstoque ?? 0;
            int diferenca = novaQuantidade - quantidadeAntiga;

            if (diferenca != 0)
            {
                var movimentacao = new MovimentacaoEstoqueModel
                {
                    ProdutoID = produto.ProdutoID,
                    MovimentacaoQtd = diferenca,
                    MovimentacaoData = DateTime.Now,
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Edicao,
                    MovimentacaoDescricao = descricao,
                    UsuarioID = usuarioId,
                };

                _movimentacaoRepository.Adicionar(movimentacao);
            }

            produto.ProdutoQtdEstoque = novaQuantidade;

            _produtoRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Estoque ajustado com sucesso!");
        }


    }
}