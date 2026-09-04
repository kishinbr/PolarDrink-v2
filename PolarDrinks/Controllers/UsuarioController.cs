using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Data;
using PolarDrinks.Filters;
using PolarDrinks.Models;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsuarioController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var usuarios = _db.Usuarios.OrderBy(u => u.UsuarioLogin).ToList();
            return View(usuarios);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(string usuarioNome, string usuarioLogin, string senha, string confirmacaoSenha, string usuarioPerfil, string senhaAtual)
        {
            if (string.IsNullOrWhiteSpace(usuarioNome))
            {
                ViewBag.Erro = "Informe o nome do usuário.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(usuarioLogin))
            {
                ViewBag.Erro = "Informe o login do usuário.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
            {
                ViewBag.Erro = "A senha deve ter pelo menos 6 caracteres.";
                return View();
            }

            if (senha != confirmacaoSenha)
            {
                ViewBag.Erro = "As senhas não conferem.";
                return View();
            }

            if (_db.Usuarios.Any(u => u.UsuarioLogin == usuarioLogin))
            {
                ViewBag.Erro = "Já existe um usuário com esse login.";
                return View();
            }

            // valida a senha do usuário logado
            var loginLogado = HttpContext.Session.GetString("Usuario");
            var usuarioLogado = _db.Usuarios.FirstOrDefault(u => u.UsuarioLogin == loginLogado);

            if (usuarioLogado == null || !BCrypt.Net.BCrypt.Verify(senhaAtual, usuarioLogado.UsuarioSenhaHash))
            {
                ViewBag.Erro = "Senha atual incorreta.";
                return View();
            }

            var perfil = usuarioPerfil == "Admin" ? "Admin" : "Funcionario";

            var novo = new UsuarioModel
            {
                UsuarioNome = usuarioNome,
                UsuarioLogin = usuarioLogin.Trim(),
                UsuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                UsuarioPerfil = perfil,
                UsuarioAtivo = true,
                UsuarioCriadoEm = DateTime.Now
            };

            _db.Usuarios.Add(novo);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = $"Usuário '{novo.UsuarioLogin}' criado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AlternarAtivo(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();


            var usuarioLogado = HttpContext.Session.GetString("Usuario");
            if (usuario.UsuarioLogin == usuarioLogado && usuario.UsuarioAtivo)
            {
                TempData["MensagemErro"] = "Você não pode desativar sua própria conta.";
                return RedirectToAction("Index");
            }

            if (usuario.UsuarioAtivo && usuario.UsuarioPerfil == "Admin"
                && _db.Usuarios.Count(u => u.UsuarioAtivo && u.UsuarioPerfil == "Admin") == 1)
            {
                TempData["MensagemErro"] = "Não é possível desativar o único administrador ativo.";
                return RedirectToAction("Index");
            }

            usuario.UsuarioAtivo = !usuario.UsuarioAtivo;
            _db.SaveChanges();

            TempData["MensagemSucesso"] = $"Usuário '{usuario.UsuarioNome}' " +
                                          (usuario.UsuarioAtivo ? "ativado" : "desativado") + ".";
            return RedirectToAction("Index");
        }

        public IActionResult AlterarSenha(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        public IActionResult AlterarSenha(int id, string senhaAtual, string novaSenha, string confirmacaoSenha)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.UsuarioSenhaHash))
            {
                ViewBag.Erro = "Senha atual incorreta.";
                return View(usuario);
            }

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
            {
                ViewBag.Erro = "A nova senha deve ter pelo menos 6 caracteres.";
                return View(usuario);
            }

            if (novaSenha != confirmacaoSenha)
            {
                ViewBag.Erro = "As senhas não conferem.";
                return View(usuario);
            }

            usuario.UsuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = $"Senha de '{usuario.UsuarioNome}' alterada com sucesso!";
            return RedirectToAction("Index");
        }
        public IActionResult Desativar(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }
        [HttpPost]
        public IActionResult Desativar(int id, string senhaAtual)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            var usuarioLogadoLogin = HttpContext.Session.GetString("Usuario");

            var usuarioLogado = _db.Usuarios
                .FirstOrDefault(u => u.UsuarioLogin == usuarioLogadoLogin);

            if (usuarioLogado == null)
                return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuarioLogado.UsuarioSenhaHash))
            {
                ViewBag.Erro = "Senha incorreta.";
                return View(usuario);
            }

            if (usuario.UsuarioLogin == usuarioLogadoLogin && usuario.UsuarioAtivo)
            {
                TempData["MensagemErro"] = "Você não pode desativar sua própria conta.";
                return RedirectToAction("Index");
            }

            if (usuario.UsuarioAtivo && usuario.UsuarioPerfil == "Admin"
                && _db.Usuarios.Count(u => u.UsuarioAtivo && u.UsuarioPerfil == "Admin") == 1)
            {
                TempData["MensagemErro"] = "Não é possível desativar o único administrador ativo.";
                return RedirectToAction("Index");
            }

            usuario.UsuarioAtivo = !usuario.UsuarioAtivo;
            _db.SaveChanges();

            TempData["MensagemSucesso"] = $"Usuário '{usuario.UsuarioNome}' " +
                                         (usuario.UsuarioAtivo ? "ativado" : "desativado") + ".";

            return RedirectToAction("Index");
        }
    }
}