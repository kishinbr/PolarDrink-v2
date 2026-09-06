using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IProdutoRepository
    {
        List<ProdutoModel> ObterTodos();
        List<ProdutoModel> ObterAtivos();
        ProdutoModel? ObterPorId(int id);
        List<ProdutoModel> ObterPorIds(List<int> ids);
        bool ExisteCodigoBarra(string codigoBarra, int? idParaIgnorar = null);
        void Adicionar(ProdutoModel produto);
        void SalvarAlteracoes();
        List<int> ObterCategoriasDoProduto(int produtoId);
        void DefinirCategoriasDoProduto(int produtoId, List<int> categoriaIds);
    }
}