using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IProdutoRepository
    {
        List<ProdutoModel> ObterTodos();
        List<ProdutoModel> ObterAtivos();
        ProdutoModel? ObterPorId(int id);
        bool ExisteCodigoBarra(string codigoBarra, int? idParaIgnorar = null);
        void Adicionar(ProdutoModel produto);
        void SalvarAlteracoes();
    }
}