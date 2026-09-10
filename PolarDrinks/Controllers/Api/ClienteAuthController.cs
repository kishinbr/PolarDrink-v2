using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Services.Loja;

namespace PolarDrinks.Controllers.Api
{
    [ApiController]
    [Route("api/cliente/auth")]
    public class ClienteAuthController : ControllerBase
    {
        private readonly IClienteAuthService _clienteAuthService;

        public ClienteAuthController(IClienteAuthService clienteAuthService)
        {
            _clienteAuthService = clienteAuthService;
        }

        public class CadastroRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
            public string ConfirmacaoSenha { get; set; } = string.Empty;
            public string Telefone { get; set; } = string.Empty;
            public string Cpf { get; set; } = string.Empty;
        }

        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] CadastroRequest request)
        {
            var resultado = _clienteAuthService.Cadastrar(
                request.Nome, request.Email, request.Senha, request.ConfirmacaoSenha,
                request.Telefone, request.Cpf);

            if (!resultado.Sucesso)
            {
                return BadRequest(new { mensagem = resultado.Mensagem, campo = resultado.CampoErro });
            }

            return Ok(new { token = resultado.Dado, mensagem = resultado.Mensagem });
        }
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var resultado = _clienteAuthService.Login(request.Email, request.Senha);

            if (!resultado.Sucesso)
            {
                return Unauthorized(new { mensagem = resultado.Mensagem });
            }

            return Ok(new { token = resultado.Dado, mensagem = resultado.Mensagem });
        }

    }
}