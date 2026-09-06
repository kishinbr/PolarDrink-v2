using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Services.Loja;

namespace PolarDrinks.Controllers.Api
{
    [ApiController]
    [Route("api/catalogo")]
    public class CatalogoController : ControllerBase
    {
        private readonly ICatalogoService _catalogoService;

        public CatalogoController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        [HttpGet("produtos")]
        public IActionResult BuscarProdutos([FromQuery] string? termo, [FromQuery] int? categoriaId)
        {
            var produtos = _catalogoService.BuscarProdutos(termo, categoriaId);
            return Ok(produtos);
        }

        [HttpGet("produtos/{id}")]
        public IActionResult ObterProduto(int id)
        {
            var produto = _catalogoService.ObterProduto(id);

            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpGet("categorias")]
        public IActionResult ListarCategorias()
        {
            var categorias = _catalogoService.ListarCategorias();
            return Ok(categorias);
        }
    }
}