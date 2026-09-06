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
        private readonly ICategoriaService _categoriaService;

        public EstoqueController(IEstoqueService estoqueService, ICategoriaService categoriaService)
        {
            _estoqueService = estoqueService;
            _categoriaService = categoriaService;
        }
        public IActionResult Index()
        {
            var produtos = _estoqueService.ListarProdutos();
            return View(produtos);
        }

        [AdminFilter]
        public IActionResult Cadastrar()
        {
            ViewBag.TodasCategorias = _categoriaService.ListarAtivas();
            return View();
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Cadastrar(ProdutoModel produto, List<int> categoriaIds, IFormFile? imagem)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao cadastrar produto.";
                return View(produto);
            }

            var resultado = _estoqueService.CadastrarProduto(produto, categoriaIds ?? new List<int>(), imagem);

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

            ViewBag.TodasCategorias = _categoriaService.ListarAtivas();
            ViewBag.CategoriasSelecionadas = _estoqueService.ObterCategoriasDoProduto(id.Value);

            return View(produto);
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Editar(ProdutoModel produto, List<int> categoriaIds, IFormFile? imagem)
        {
            if (!ModelState.IsValid)
            {
                var produtoOriginal = _estoqueService.ObterProduto(produto.ProdutoID);
                return View(produtoOriginal);
            }

            var resultado = _estoqueService.EditarProduto(produto, categoriaIds ?? new List<int>(), imagem);

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