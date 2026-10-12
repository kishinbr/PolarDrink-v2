using PolarDrinks.Models;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IEstoqueService
    {
        List<ProdutoModel> ListarProdutos();
        List<ProdutoModel> ListarProdutosAtivos();
        ProdutoModel? ObterProduto(int id);

        ResultadoOperacao CadastrarProduto(ProdutoModel produto, List<int> categoriaIds, (Stream Conteudo, string NomeArquivo)? imagem);
        ResultadoOperacao EditarProduto(ProdutoModel produto, List<int> categoriaIds, (Stream Conteudo, string NomeArquivo)? imagem);
        ResultadoOperacao EdicaoRapida(int produtoId, decimal? precoVenda, decimal? promocao);
        ResultadoOperacao AjustarEstoque(int produtoId, int novaQuantidade, string descricao, int? usuarioId);

        List<MovimentacaoEstoqueModel> ObterMovimentacoes(int produtoId);
        List<int> ObterCategoriasDoProduto(int produtoId);
        (int SemEstoque, int AbaixoDoMinimo) ContarAlertasDeEstoque();
    }
}