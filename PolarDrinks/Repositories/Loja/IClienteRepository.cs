using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
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