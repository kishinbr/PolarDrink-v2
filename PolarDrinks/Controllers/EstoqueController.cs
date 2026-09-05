using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Models;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class EstoqueController : Controller
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }
        public IActionResult Index()
        {
            var produtos = _estoqueService.ListarProdutos();
            return View(produtos);
        }

        [AdminFilter]
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Cadastrar(ProdutoModel produto)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao cadastrar produto.";
                return View(produto);
            }

            var resultado = _estoqueService.CadastrarProduto(produto);

            if (!resultado.Sucesso)
            {
                if (resultado.CampoErro != null)
                {
                    ModelState.AddModelError(resultado.CampoErro, resultado.Mensagem!);
                }
                TempData["MensagemErro"] = "Erro ao cadastrar produto.";
                return View(produto);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AdminFilter]
        public IActionResult Editar(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var produto = _estoqueService.ObterProduto(id.Value);
            if (produto == null) return NotFound();

            return View(produto);
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Editar(ProdutoModel produto)
        {
            if (!ModelState.IsValid)
            {
                var produtoOriginal = _estoqueService.ObterProduto(produto.ProdutoID);
                return View(produtoOriginal);
            }

            var resultado = _estoqueService.EditarProduto(produto);

            if (!resultado.Sucesso)
            {
                if (resultado.CampoErro != null)
                {
                    ModelState.AddModelError(resultado.CampoErro, resultado.Mensagem!);
                    return View(produto);
                }

                return NotFound();
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult EdicaoRapida(int ProdutoID, decimal? ProdutoPrecoVenda, decimal? ProdutoPromocao)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Valores inválidos!";
                return RedirectToAction("Index");
            }

            var resultado = _estoqueService.EdicaoRapida(ProdutoID, ProdutoPrecoVenda, ProdutoPromocao);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult AjustarEstoque(int ProdutoID, int NovaQuantidade, string Descricao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _estoqueService.AjustarEstoque(ProdutoID, NovaQuantidade, Descricao, usuarioId);

            TempData[resultado.Sucesso ? "MensagemSucesso" : "MensagemErro"] = resultado.Mensagem;
            return RedirectToAction("Editar", new { id = ProdutoID });
        }

        [AdminFilter]
        public IActionResult Movimentacoes(int? produtoId)
        {
            var produtos = _estoqueService.ListarProdutosAtivos();
            ViewBag.Produtos = produtos;

            if (produtoId == null)
            {
                return View(new List<MovimentacaoEstoqueModel>());
            }

            var produto = _estoqueService.ObterProduto(produtoId.Value);
            if (produto == null)
            {
                return View(new List<MovimentacaoEstoqueModel>());
            }

            var movimentacoes = _estoqueService.ObterMovimentacoes(produtoId.Value);

            ViewBag.ProdutoSelecionado = produto;

            return View(movimentacoes);
        }
    }
}