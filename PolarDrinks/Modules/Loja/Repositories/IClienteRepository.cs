using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Repositories
{
    public interface IClienteRepository
    {
        ClienteModel? ObterPorId(int id);
        ClienteModel? ObterPorEmail(string email);
        bool ExisteEmail(string email);
        bool ExisteCPF(string cpf);

        void Adicionar(ClienteModel cliente);
        void SalvarAlteracoes();
    }
}