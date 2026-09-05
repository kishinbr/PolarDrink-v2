using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Index()
        {
            var usuarios = _usuarioService.ListarUsuarios();
            return View(usuarios);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(string usuarioNome, string usuarioLogin, string senha, string confirmacaoSenha, string usuarioPerfil, string senhaAtual)
        {
            var loginLogado = HttpContext.Session.GetString("Usuario");

            var resultado = _usuarioService.CadastrarUsuario(
                usuarioNome, usuarioLogin, senha, confirmacaoSenha, usuarioPerfil, senhaAtual, loginLogado);

            if (!resultado.Sucesso)
            {
                ViewBag.Erro = resultado.Mensagem;
                return View();
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        public IActionResult AlterarSenha(int id)
        {
            var usuario = _usuarioService.ObterUsuario(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        public IActionResult AlterarSenha(int id, string senhaAtual, string novaSenha, string confirmacaoSenha)
        {
            var usuario = _usuarioService.ObterUsuario(id);
            if (usuario == null)
                return NotFound();

            var resultado = _usuarioService.AlterarSenha(id, senhaAtual, novaSenha, confirmacaoSenha);

            if (!resultado.Sucesso)
            {
                ViewBag.Erro = resultado.Mensagem;
                return View(usuario);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        public IActionResult Desativar(int id)
        {
            var usuario = _usuarioService.ObterUsuario(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Desativar(int id, string senhaAtual)
        {
            var usuario = _usuarioService.ObterUsuario(id);
            if (usuario == null)
                return NotFound();

            var loginLogado = HttpContext.Session.GetString("Usuario");

            var resultado = _usuarioService.Desativar(id, senhaAtual, loginLogado);

            if (!resultado.Sucesso)
            {
                if (resultado.CampoErro == "REDIRECT")
                {
                    TempData["MensagemErro"] = resultado.Mensagem;
                    return RedirectToAction("Index");
                }

                ViewBag.Erro = resultado.Mensagem;
                return View(usuario);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }
    }
}