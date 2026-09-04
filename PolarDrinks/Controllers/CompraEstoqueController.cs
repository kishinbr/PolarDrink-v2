using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Filters;
using PolarDrinks.Models;
using PolarDrinks.ViewModels;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class CompraEstoqueController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CompraEstoqueController(ApplicationDbContext db)
        {
            _db = db;
        }

        // metodo para recarregar as listas de fornecedores e produtos 
        private void RecarregarListas(CompraCreateViewModel vm)
        {
            vm.Fornecedores = _db.Fornecedores
                .Where(f => f.FornecedorAtivo)  
                .Select(f => new SelectListItem
                {
                    Value = f.FornecedorID.ToString(),
                    Text = f.FornecedorNome
                }).ToList();

            vm.Produtos = _db.Produtos
                .Select(p => new SelectListItem
                {
                    Value = p.ProdutoID.ToString(),
                    Text = p.ProdutoNome
                }).ToList();
            vm.ProdutosEstoque = _db.Produtos
                .Where(p => p.ProdutoAtivo)
                .OrderBy(p => p.ProdutoNome)
                .ToList();
        }
        //index para listar as compras, separando por status (pendentes e concluidas)
        public IActionResult Index()
        {
            var vm = new CompraIndexViewModel
            {
                Pendentes = _db.ComprasEstoque
                    .Include(c => c.Fornecedor)
                    .Where(c => c.CompraStatus == "Aguardando")
                    .OrderByDescending(c => c.CompraData)
                    .ToList(),

                Concluidas = _db.ComprasEstoque
                    .Include(c => c.Fornecedor)
                    .Where(c => c.CompraStatus == "Concluído")
                    .OrderByDescending(c => c.CompraData)
                    .ToList()
            };

            return View(vm);
        }
        private void PreencherNomesProdutos(CompraCreateViewModel vm)
        {
            var ids = vm.Itens.Where(i => i.ProdutoID.HasValue).Select(i => i.ProdutoID.Value).ToList();
            var produtos = _db.Produtos.Where(p => ids.Contains(p.ProdutoID)).ToList();

            foreach (var item in vm.Itens)
            {
                if (item.ProdutoID.HasValue)
                {
                    item.ProdutoNome = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID)?.ProdutoNome;
                }
            }
        }
        public IActionResult Cadastrar()
        {
            var vm = new CompraCreateViewModel
            {
                Fornecedores = _db.Fornecedores
                    .Where(f => f.FornecedorAtivo)
                    .Select(f => new SelectListItem
                    {
                        Value = f.FornecedorID.ToString(),
                        Text = f.FornecedorNome
                    }).ToList(),

                Produtos = _db.Produtos
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProdutoID.ToString(),
                        Text = p.ProdutoNome
                    }).ToList(),

                Itens = new List<ItemCompraCreateVM>(),

                ProdutosEstoque = _db.Produtos
                    .Where(p => p.ProdutoAtivo)
                    .OrderBy(p => p.ProdutoNome)
                    .ToList()
            };
            
            return View(vm);
        }
        // metodo para cadastrar a compra, validando os dados e adicionando os itens
        [HttpPost]
        public IActionResult Cadastrar(CompraCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PreencherNomesProdutos(vm);
                RecarregarListas(vm);
                return View(vm);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            var compra = new CompraEstoqueModel
            {
                FornecedorID = vm.FornecedorID.Value,
                CompraData = DateTime.Now,
                CompraStatus = "Aguardando",
                UsuarioID = usuarioId,
                Itens = new List<ItemCompraModel>()
            };

            foreach (var item in vm.Itens)
            {
                if (item.ProdutoID.HasValue && item.Quantidade > 0 && item.Preco > 0)
                {
                    compra.Itens.Add(new ItemCompraModel
                    {
                        ProdutoID = item.ProdutoID.Value,
                        ItemCompraQtd = item.Quantidade,
                        ItemCompraPreco = item.Preco
                    });
                }
            }

            if (compra.Itens.Count == 0)
            {
                PreencherNomesProdutos(vm);
                ModelState.AddModelError("", "Adicione pelo menos um produto");
                RecarregarListas(vm);
                return View(vm);
            }

            _db.ComprasEstoque.Add(compra);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Compra cadastrada com sucesso!";
            return RedirectToAction("Index");
        }
        // detalhes da compra, mostrando os itens e o fornecedor, e se pode confirmar a entrega ou excluir
        public IActionResult Detalhes(int id, bool confirmar = false)
        {
            var compra = _db.ComprasEstoque
                .Include(c => c.Fornecedor)
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.CompraID == id);

            if (compra == null)
                return NotFound();

            var vm = new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = confirmar
            };
            var usuarioConfirmacao = _db.MovimentacoesEstoque
                .Include(m => m.Usuario)
                .Where(m => m.ItemCompra.CompraID == id
                         && m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Entrada)
                .Select(m => m.Usuario.UsuarioNome)
                .FirstOrDefault();

            ViewBag.UsuarioConfirmacao = usuarioConfirmacao;

            return View(vm);
        }

        public IActionResult ConfirmarEntrega(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            var compra = _db.ComprasEstoque
                .Include(c => c.Itens)
                .FirstOrDefault(c => c.CompraID == id);

            if (compra == null)
                return NotFound();

            if (compra.CompraStatus == "Concluído")
                return RedirectToAction("Index");

            var ids = compra.Itens.Select(i => i.ProdutoID).ToList();
            var produtos = _db.Produtos
                .Where(p => ids.Contains(p.ProdutoID))
                .ToList();

            foreach (var item in compra.Itens)
            {
                var produto = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID);

                if (produto == null) continue;

                produto.ProdutoQtdEstoque += item.ItemCompraQtd;

                var movimentacao = new MovimentacaoEstoqueModel
                {
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Entrada,
                    MovimentacaoQtd = item.ItemCompraQtd,
                    ProdutoID = produto.ProdutoID,
                    ItemCompraID = item.ItemCompraID,
                    MovimentacaoData = DateTime.Now,
                    UsuarioID = usuarioId
                };

                _db.MovimentacoesEstoque.Add(movimentacao);
            }

            compra.CompraStatus = "Concluído";
            compra.CompraDataEntrega = DateTime.Now;
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Entrega confirmada e estoque atualizado!";
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id)
        {
            var compra = _db.ComprasEstoque
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .Include(c => c.Fornecedor)
                .FirstOrDefault(c => c.CompraID == id);

            if (compra == null)
                return NotFound();

            var vm = new CompraDetalhesViewModel
            {
                Compra = compra,
                Itens = compra.Itens.ToList(),
                PodeConfirmar = false
            };

            return View(vm);
        }

        [HttpPost]
        
        public IActionResult ConfirmarExclusao(int id)
        {
            var compra = _db.ComprasEstoque
                .Include(c => c.Itens)
                .FirstOrDefault(c => c.CompraID == id);

            if (compra == null)
                return NotFound();

            _db.ComprasEstoque.Remove(compra);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Compra excluída com sucesso!";
            return RedirectToAction("Index");
        }
    }
}