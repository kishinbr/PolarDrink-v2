using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Models;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class FornecedorController : Controller
    {
        private readonly IFornecedorService _fornecedorService;

        public FornecedorController(IFornecedorService fornecedorService)
        {
            _fornecedorService = fornecedorService;
        }

        public IActionResult Index()
        {
            var fornecedores = _fornecedorService.ListarFornecedores();
            return View(fornecedores);
        }

        [HttpGet]
        [AdminFilter]
        public IActionResult Editar(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var fornecedor = _fornecedorService.ObterFornecedor(id.Value);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Editar(FornecedorModel fornecedor)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao editar fornecedor.";
                return View(fornecedor);
            }

            var resultado = _fornecedorService.EditarFornecedor(fornecedor);

            if (!resultado.Sucesso)
            {
                if (resultado.CampoErro != null)
                {
                    ModelState.AddModelError(resultado.CampoErro, resultado.Mensagem!);
                    return View(fornecedor);
                }

                TempData["MensagemErro"] = resultado.Mensagem;
                return View();
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [AdminFilter]
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Cadastrar(FornecedorModel fornecedor)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao cadastrar fornecedor.";
                return View(fornecedor);
            }

            var resultado = _fornecedorService.CadastrarFornecedor(fornecedor);

            if (!resultado.Sucesso)
            {
                if (resultado.CampoErro != null)
                {
                    ModelState.AddModelError(resultado.CampoErro, resultado.Mensagem!);
                }
                return View(fornecedor);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }
    }
}