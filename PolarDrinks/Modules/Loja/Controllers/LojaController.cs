using Microsoft.AspNetCore.Mvc;

namespace PolarDrinks.Modules.Loja.Controllers
{
    public class LojaController : Controller
    {
        public IActionResult Catalogo()
        {
            return View();
        }

        public IActionResult Conta()
        {
            return View();
        }

        public IActionResult Carrinho()
        {
            return View();
        }
        public IActionResult PedidoConfirmado()
        {
            return View();
        }
        public IActionResult Perfil()
        {
            return View();
        }
    }
}