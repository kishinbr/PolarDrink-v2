using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PolarDrinks.Filters;
using PolarDrinks.Models;
using PolarDrinks.Services;
using PolarDrinks.ViewModels;

namespace PolarDrinks.Controllers
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

            vm.Produtos = _compraService.ListarTodosProdutos()
                .Select(p => new SelectListItem
                {
                    Value = p.ProdutoID.ToString(),
                    Text = p.ProdutoNome
                }).ToList();

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
                _compraService.PreencherNomesProdutos(vm.Itens);
                RecarregarListas(vm);
                return View(vm);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var resultado = _compraService.CadastrarCompra(vm.FornecedorID, vm.Itens, usuarioId);

            if (!resultado.Sucesso)
            {
                _compraService.PreencherNomesProdutos(vm.Itens);
                ModelState.AddModelError(resultado.CampoErro ?? "", resultado.Mensagem!);
                RecarregarListas(vm);
                return View(vm);
            }

            TempData["MensagemSucesso"] = resultado.Mensagem;
            return RedirectToAction("Index");
        }


        public IActionResult Detalhes(int id, bool confirmar = false)
        {
            var vm = _compraService.ObterDetalhes(id, confirmar);

            if (vm == null)
                return NotFound();

            ViewBag.UsuarioConfirmacao = vm.UsuarioConfirmacao;

            return View(vm);
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
            var vm = _compraService.ObterParaExcluir(id);

            if (vm == null)
                return NotFound();

            return View(vm);
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