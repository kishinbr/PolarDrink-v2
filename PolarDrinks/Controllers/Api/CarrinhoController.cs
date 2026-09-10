using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Services.Loja;
using System.Security.Claims;

namespace PolarDrinks.Controllers.Api
{
    [ApiController]
    [Route("api/carrinho")]
    [Authorize]
    public class CarrinhoController : ControllerBase
    {
        private readonly ICarrinhoService _carrinhoService;

        public CarrinhoController(ICarrinhoService carrinhoService)
        {
            _carrinhoService = carrinhoService;
        }

        private int ObterClienteId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return int.Parse(claim);
        }
        [HttpGet]
        public IActionResult ObterCarrinho()
        {
            var carrinho = _carrinhoService.ObterCarrinho(ObterClienteId());
            return Ok(carrinho);
        }

        public class AdicionarItemRequest
        {
            public int ProdutoID { get; set; }
            public int Quantidade { get; set; } = 1;
        }

        [HttpPost("itens")]
        public IActionResult AdicionarItem([FromBody] AdicionarItemRequest request)
        {
            _carrinhoService.AdicionarItem(ObterClienteId(), request.ProdutoID, request.Quantidade);
            var carrinho = _carrinhoService.ObterCarrinho(ObterClienteId());
            return Ok(carrinho);
        }

        public class AtualizarQuantidadeRequest
        {
            public int Quantidade { get; set; }
        }

        [HttpPut("itens/{produtoId}")]
        public IActionResult AtualizarQuantidade(int produtoId, [FromBody] AtualizarQuantidadeRequest request)
        {
            _carrinhoService.AtualizarQuantidade(ObterClienteId(), produtoId, request.Quantidade);
            var carrinho = _carrinhoService.ObterCarrinho(ObterClienteId());
            return Ok(carrinho);
        }

        [HttpDelete("itens/{produtoId}")]
        public IActionResult RemoverItem(int produtoId)
        {
            _carrinhoService.RemoverItem(ObterClienteId(), produtoId);
            var carrinho = _carrinhoService.ObterCarrinho(ObterClienteId());
            return Ok(carrinho);
        }

        [HttpDelete]
        public IActionResult LimparCarrinho()
        {
            _carrinhoService.LimparCarrinho(ObterClienteId());
            return Ok(new { mensagem = "Carrinho esvaziado." });
        }

        [HttpPost("mesclar")]
        public IActionResult MesclarCarrinho([FromBody] List<ItemMesclagemDto> itens)
        {
            _carrinhoService.MesclarCarrinho(ObterClienteId(), itens);
            var carrinho = _carrinhoService.ObterCarrinho(ObterClienteId());
            return Ok(carrinho);
        }
    }
}