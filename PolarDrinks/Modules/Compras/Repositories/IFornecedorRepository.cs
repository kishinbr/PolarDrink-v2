using PolarDrinks.Modules.Compras.Models;

namespace PolarDrinks.Modules.Compras.Repositories
{
    public interface IFornecedorRepository
    {
        List<FornecedorModel> ObterTodos();
        List<FornecedorModel> ObterAtivos();
        FornecedorModel? ObterPorId(int id);
        bool ExisteCNPJ(string cnpj, int? idParaIgnorar = null);
        void Adicionar(FornecedorModel fornecedor);
        void SalvarAlteracoes();

    }
}