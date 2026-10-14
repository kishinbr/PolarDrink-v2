using PolarDrinks.Modules.Estoque.Models;

namespace PolarDrinks.Modules.Estoque.Repositories
{
    public interface ICategoriaRepository
    {
        List<CategoriaModel> ObterTodas();
        List<CategoriaModel> ObterAtivas();
        CategoriaModel? ObterPorId(int id);
        bool ExisteNome(string nome, int? idParaIgnorar = null);

        void Adicionar(CategoriaModel categoria);
        void SalvarAlteracoes();
    }
}