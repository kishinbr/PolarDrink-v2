using Microsoft.AspNetCore.Mvc;

namespace PolarDrinks.Controllers.Loja
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
    }
}