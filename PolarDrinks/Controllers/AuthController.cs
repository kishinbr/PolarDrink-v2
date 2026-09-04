using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Data;
using PolarDrinks.Filters;

namespace PolarDrinks.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string senha)
        {
            var user = _db.Usuarios
                .FirstOrDefault(u => u.UsuarioLogin == usuario && u.UsuarioAtivo);

            if (user != null && BCrypt.Net.BCrypt.Verify(senha, user.UsuarioSenhaHash))
            {
                HttpContext.Session.SetString("Logado", "true");
                HttpContext.Session.SetInt32("UsuarioID", user.UsuarioID);
                HttpContext.Session.SetString("Usuario", user.UsuarioLogin);
                HttpContext.Session.SetString("Perfil", user.UsuarioPerfil);
                HttpContext.Session.SetString("Nome", user.UsuarioNome);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = "Usuário ou senha inválidos";
            return View();
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