using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;

namespace PolarDrinks.Services.Loja
{
    public class CatalogoService : ICatalogoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CatalogoService(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = categoriaRepository;
        }

        public List<CatalogoProdutoDto> BuscarProdutos(string? termo, int? categoriaId)
        {
            return _produtoRepository.ObterCatalogo(termo, categoriaId);
        }

        public CatalogoProdutoDto? ObterProduto(int id)
        {
            return _produtoRepository.ObterCatalogoPorId(id);
        }

        public List<CategoriaModel> ListarCategorias()
        {
            return _categoriaRepository.ObterAtivas();
        }
    }
}