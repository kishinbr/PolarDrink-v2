using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PolarDrinks.Compartilhado.Filters;
using PolarDrinks.Modules.Compras.Models;
using PolarDrinks.Modules.Compras.Services;

namespace PolarDrinks.Modules.Compras.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class CompraEstoqueController : Controller
    {
        private readonly ICompraEstoqueService _compraService;

        public CompraEstoqueController(ICompraEstoqueService compraService)
        {
            _compraService = compraService;
        }


        private void RecarregarListas(CompraCreateViewModel vm)
        {
            vm.Fornecedores = _compraService.ListarFornecedoresAtivos()
                .Select(f => new SelectListItem
                {
                    Value = f.FornecedorID.ToString(),
                    Text = f.FornecedorNome
                }).ToList();

            var produtos = _compraService.ListarTodosProdutos();

            vm.Produtos = produtos
                .Select(p => new SelectListItem
                {
                    Value = p.ProdutoID.ToString(),
                    Text = p.ProdutoNome
                }).ToList();

            foreach (var item in vm.Itens.Where(i => i.ProdutoID.HasValue))
            {
                item.ProdutoNome = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID)?.ProdutoNome;
            }

            vm.ProdutosEstoque = _compraService.ListarProdutosAtivosOrdenados();
        }


        public IActionResult Index()
        {
            var (pendentes, concluidas) = _compraService.ListarComprasPorStatus();

            var vm = new CompraIndexViewModel
            {
                Pendentes = pendentes,
                Concluidas = concluidas
            };

            return View(vm);
        }

        public IActionResult Cadastrar()
        {
            var vm = new CompraCreateViewModel
            {
                Itens = new List<ItemCompraCreateVM>()
            };

            RecarregarListas(vm);

            return View(vm);
        }

        [HttpPost]
        public IActionResult Cadastrar(CompraCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                RecarregarListas(vm);
                return View(vm);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var itens = vm.Itens.Select(i => (i.ProdutoID, i.Quantidade, i.Preco));

            var resultado = _compraService.CadastrarCompra(vm.FornecedorID, itens, usuarioId);

            if (!resultado.Sucesso)
            {
                ModelState.AddModelError(resultado.CampoErro ?? "", resultado.Mensagem!);
                RecarregarListas(vm);
                return View(vm);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }


        public IActionResult Detalhes(int id, bool confirmar = false)
        {
            var detalhes = _compraService.ObterDetalhes(id);

            if (detalhes == null)
                return NotFound();

            var (compra, usuarioConfirmacao) = detalhes.Value;

            ViewBag.UsuarioConfirmacao = usuarioConfirmacao;

            return View(new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = confirmar,
                UsuarioConfirmacao = usuarioConfirmacao
            });
        }

        public IActionResult ConfirmarEntrega(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _compraService.ConfirmarEntrega(id, usuarioId);

            if (!resultado.Sucesso)
                return NotFound();

            if (resultado.Mensagem != null)
                TempData["MensagemSucesso"] = resultado.Mensagem;

            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id)
        {
            var compra = _compraService.ObterParaExcluir(id);

            if (compra == null)
                return NotFound();

            return View(new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = false
            });
        }

        [HttpPost]
        public IActionResult ConfirmarExclusao(int id)
        {
            var resultado = _compraService.ConfirmarExclusao(id);

            if (!resultado.Sucesso)
                return NotFound();

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }
    }
}