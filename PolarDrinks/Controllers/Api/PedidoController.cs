using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Services.Loja;
using System.Security.Claims;

namespace PolarDrinks.Controllers.Api
{
    [ApiController]
    [Route("api/pedidos")]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        private int ObterClienteId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return int.Parse(claim);
        }

        [HttpPost("checkout")]
        public IActionResult Checkout()
        {
            var resultado = _pedidoService.Checkout(ObterClienteId());

            if (!resultado.Sucesso)
            {
                return BadRequest(new { mensagem = resultado.Mensagem });
            }

            return Ok(resultado.Dado);
        }

        [HttpGet]
        public IActionResult ListarPedidos()
        {
            var pedidos = _pedidoService.ListarPedidosDoCliente(ObterClienteId());
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public IActionResult ObterDetalhe(int id)
        {
            var pedido = _pedidoService.ObterDetalhePedido(ObterClienteId(), id);

            if (pedido == null)
            {
                return NotFound();
            }

            return Ok(pedido);
        }

        [HttpPost("{id}/cancelar")]
        public IActionResult Cancelar(int id)
        {
            var resultado = _pedidoService.CancelarPeloCliente(ObterClienteId(), id);

            if (!resultado.Sucesso)
            {
                return BadRequest(new { mensagem = resultado.Mensagem });
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }
    }
}