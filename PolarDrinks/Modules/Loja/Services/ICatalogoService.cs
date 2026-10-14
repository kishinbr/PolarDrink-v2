using PolarDrinks.Modules.Estoque.Models;

namespace PolarDrinks.Modules.Loja.Services
{
    public interface ICatalogoService
    {
        List<CatalogoProdutoDto> BuscarProdutos(string? termo, int? categoriaId);
        CatalogoProdutoDto? ObterProduto(int id);
        List<CategoriaModel> ListarCategorias();
    }
}