using PolarDrinks.Models.Loja;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface ICategoriaService
    {
        List<CategoriaModel> ListarCategorias();
        List<CategoriaModel> ListarAtivas();
        CategoriaModel? ObterCategoria(int id);

        ResultadoOperacao CadastrarCategoria(string nome);
        ResultadoOperacao EditarCategoria(int id, string nome, bool ativa);
    }
}