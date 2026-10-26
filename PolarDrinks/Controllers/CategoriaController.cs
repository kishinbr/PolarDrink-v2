using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Models.Loja;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        public IActionResult Index()
        {
            var categorias = _categoriaService.ListarCategorias();
            return View(categorias);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(string categoriaNome)
        {
            var resultado = _categoriaService.CadastrarCategoria(categoriaNome);

            if (!resultado.Sucesso)
            {
                ModelState.AddModelError(resultado.CampoErro ?? "", resultado.Mensagem!);
                return View();
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var categoria = _categoriaService.ObterCategoria(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost]
        public IActionResult Editar(CategoriaModel categoria)
        {
            var resultado = _categoriaService.EditarCategoria(categoria.CategoriaID, categoria.CategoriaNome, categoria.CategoriaAtiva);

            if (!resultado.Sucesso)
            {
                ModelState.AddModelError(resultado.CampoErro ?? "", resultado.Mensagem!);
                return View(categoria);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }
    }
}