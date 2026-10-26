using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Models;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class VendaController : Controller
    {
        private readonly IVendaService _vendaService;

        public VendaController(IVendaService vendaService)
        {
            _vendaService = vendaService;
        }

        [AdminFilter]
        public IActionResult Index(DateTime? dataInicio, DateTime? dataFim)
        {
            var vendas = _vendaService.ListarVendas(dataInicio, dataFim);
            return View(vendas);
        }

        [AdminFilter]
        public IActionResult Detalhes(int id)
        {
            var detalhes = _vendaService.ObterDetalhesVenda(id);

            if (detalhes == null)
                return NotFound();

            ViewBag.MotivoCancelamento = detalhes.MotivoCancelamento;
            ViewBag.UsuarioCancelamento = detalhes.UsuarioCancelamento;

            return View(detalhes.Venda);
        }

        public IActionResult Cadastrar()
        {
            ViewBag.Produtos = _vendaService.ListarProdutosAtivos();
            return View();
        }

        [HttpPost]
        public IActionResult FinalizarVenda(VendaModel venda)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _vendaService.FinalizarVenda(venda, usuarioId);

            if (!resultado.Sucesso)
            {
                TempData["MensagemErro"] = resultado.Mensagem;
                ViewBag.Produtos = _vendaService.ListarProdutosAtivos();
                return View("Cadastrar", venda);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Cadastrar");
        }

        [AdminFilter]
        public IActionResult Cancelar(int id)
        {
            var venda = _vendaService.ObterVendaParaCancelamento(id);

            if (venda == null)
                return NotFound();

            return View(venda);
        }

        [AdminFilter]
        [HttpPost]
        public IActionResult CancelarVenda(int id, string? descricao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _vendaService.CancelarVenda(id, descricao, usuarioId);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }
    }
}