using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Compras.Models;

namespace PolarDrinks.Modules.Compras.Services
{
    public interface IFornecedorService
    {
        List<FornecedorModel> ListarFornecedores();
        FornecedorModel? ObterFornecedor(int id);

        ResultadoOperacao CadastrarFornecedor(FornecedorModel fornecedor);
        ResultadoOperacao EditarFornecedor(FornecedorModel fornecedor);
    }
}