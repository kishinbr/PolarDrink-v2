using PolarDrinks.Models.Loja;

namespace PolarDrinks.Services.Loja
{
    public interface ICatalogoService
    {
        List<CatalogoProdutoDto> BuscarProdutos(string? termo, int? categoriaId);
        CatalogoProdutoDto? ObterProduto(int id);
        List<CategoriaModel> ListarCategorias();
    }
}