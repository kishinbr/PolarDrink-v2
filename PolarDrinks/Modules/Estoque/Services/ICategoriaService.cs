using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Estoque.Models;

namespace PolarDrinks.Modules.Estoque.Services
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