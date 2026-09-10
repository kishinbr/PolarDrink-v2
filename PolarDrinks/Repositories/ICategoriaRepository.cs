using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories
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