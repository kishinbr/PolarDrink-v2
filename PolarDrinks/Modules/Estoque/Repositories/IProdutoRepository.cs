using PolarDrinks.Modules.Estoque.Models;

namespace PolarDrinks.Modules.Estoque.Repositories
{
    public interface IProdutoRepository
    {
        List<ProdutoModel> ObterTodos();
        List<ProdutoModel> ObterAtivos();
        int ContarSemEstoque();
        int ContarAbaixoDoMinimo();
        ProdutoModel? ObterPorId(int id);
        List<ProdutoModel> ObterPorIds(List<int> ids);
        bool ExisteCodigoBarra(string codigoBarra, int? idParaIgnorar = null);
        void Adicionar(ProdutoModel produto);
        void SalvarAlteracoes();
        List<int> ObterCategoriasDoProduto(int produtoId);
        void DefinirCategoriasDoProduto(int produtoId, List<int> categoriaIds);

        List<CatalogoProdutoDto> ObterCatalogo(string? termo, int? categoriaId);
        CatalogoProdutoDto? ObterCatalogoPorId(int produtoId);

    }
}