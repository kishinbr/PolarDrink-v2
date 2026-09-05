using PolarDrinks.Models;

namespace PolarDrinks.Repositories
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