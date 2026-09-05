using PolarDrinks.Models;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IEstoqueService
    {
        List<ProdutoModel> ListarProdutos();
        List<ProdutoModel> ListarProdutosAtivos();
        ProdutoModel? ObterProduto(int id);

        ResultadoOperacao CadastrarProduto(ProdutoModel produto);
        ResultadoOperacao EditarProduto(ProdutoModel produto);
        ResultadoOperacao EdicaoRapida(int produtoId, decimal? precoVenda, decimal? promocao);
        ResultadoOperacao AjustarEstoque(int produtoId, int novaQuantidade, string descricao, int? usuarioId);

        List<MovimentacaoEstoqueModel> ObterMovimentacoes(int produtoId);
    }
}