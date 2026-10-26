using PolarDrinks.Models;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IFornecedorService
    {
        List<FornecedorModel> ListarFornecedores();
        FornecedorModel? ObterFornecedor(int id);

        ResultadoOperacao CadastrarFornecedor(FornecedorModel fornecedor);
        ResultadoOperacao EditarFornecedor(FornecedorModel fornecedor);
    }
}