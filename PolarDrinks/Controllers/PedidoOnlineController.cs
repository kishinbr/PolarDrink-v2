using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Models.Loja;
using PolarDrinks.Services.Loja;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class PedidoOnlineController : Controller
    {
        private readonly IPedidoService _pedidoService;

        public PedidoOnlineController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        public IActionResult Index()
        {
            var aguardando = _pedidoService.ListarPorStatus(PedidoModel.Status.AguardandoSeparacao);
            var separados = _pedidoService.ListarPorStatus(PedidoModel.Status.Separado);

            ViewBag.Separados = separados;

            return View(aguardando);
        }

        [HttpPost]
        public IActionResult MarcarComoSeparado(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _pedidoService.MarcarComoSeparado(id, usuarioId ?? 0);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult VoltarParaSeparacao(int id)
        {
            var resultado = _pedidoService.VoltarParaSeparacao(id);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ConfirmarEntrega(int id, string codigo)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _pedidoService.ConfirmarEntrega(id, codigo, usuarioId ?? 0);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        public IActionResult Historico()
        {
            var concluidos = _pedidoService.ListarPorStatus(PedidoModel.Status.Concluido);
            var canceladosCliente = _pedidoService.ListarPorStatus(PedidoModel.Status.CanceladoCliente);
            var canceladosExpirados = _pedidoService.ListarPorStatus(PedidoModel.Status.CanceladoNaoRetirado);

            ViewBag.CanceladosCliente = canceladosCliente;
            ViewBag.CanceladosExpirados = canceladosExpirados;

            return View(concluidos);
        }
    }
}