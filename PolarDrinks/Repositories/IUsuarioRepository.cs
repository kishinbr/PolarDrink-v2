using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IUsuarioRepository
    {
        List<UsuarioModel> ObterTodosOrdenados();
        UsuarioModel? ObterPorId(int id);
        UsuarioModel? ObterPorLogin(string login);
        bool ExisteLogin(string login);
        int ContarAdminsAtivos();

        void Adicionar(UsuarioModel usuario);
        void SalvarAlteracoes();
    }
}