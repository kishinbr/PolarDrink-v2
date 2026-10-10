using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string senha)
        {
            var resultado = _usuarioService.Autenticar(usuario, senha);

            if (!resultado.Sucesso)
            {
                ViewBag.Erro = resultado.Mensagem;
                return View();
            }

            var user = resultado.Dado!;

            HttpContext.Session.SetString("Logado", "true");
            HttpContext.Session.SetInt32("UsuarioID", user.UsuarioID);
            HttpContext.Session.SetString("Usuario", user.UsuarioLogin);
            HttpContext.Session.SetString("Perfil", user.UsuarioPerfil);
            HttpContext.Session.SetString("Nome", user.UsuarioNome);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Deslogar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}