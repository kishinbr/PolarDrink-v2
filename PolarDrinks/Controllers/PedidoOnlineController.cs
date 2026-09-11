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
        public IActionResult Detalhes(int id)
        {
            var pedido = _pedidoService.ObterPedidoAdmin(id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
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
        [AdminFilter]
        [HttpPost]
        public IActionResult CancelarAposEntrega(int id, string descricao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _pedidoService.CancelarAposEntrega(id, descricao, usuarioId ?? 0);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Historico");
        }
        public IActionResult Historico()
        {
            var todos = new List<PedidoModel>();
            todos.AddRange(_pedidoService.ListarPorStatus(PedidoModel.Status.Concluido));
            todos.AddRange(_pedidoService.ListarPorStatus(PedidoModel.Status.CanceladoCliente));
            todos.AddRange(_pedidoService.ListarPorStatus(PedidoModel.Status.CanceladoNaoRetirado));
            todos.AddRange(_pedidoService.ListarPorStatus(PedidoModel.Status.CanceladoAdmin));

            todos = todos.OrderByDescending(p => p.PedidoData).ToList();

            return View(todos);
        }
    }
}