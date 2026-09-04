using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Filters;
using PolarDrinks.Models;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class VendaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VendaController(ApplicationDbContext db)
        {
            _db = db;
        }
        [AdminFilter]
        // Ação para exibir a lista de vendas, com filtros opcionais por data
        public IActionResult Index(DateTime? dataInicio, DateTime? dataFim)
        {
            var vendasQuery = _db.Vendas
                                 .Include(v => v.Itens)
                                 .AsQueryable();

            if (dataInicio.HasValue)
                vendasQuery = vendasQuery.Where(v => v.VendaData.Date >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                vendasQuery = vendasQuery.Where(v => v.VendaData.Date <= dataFim.Value.Date);

            var vendas = vendasQuery.OrderByDescending(v => v.VendaData).ToList();
            return View(vendas);
        }
        // Ação para exibir os detalhes de uma venda específica, incluindo os itens e produtos relacionados
        [AdminFilter]
        public IActionResult Detalhes(int id)
        {
            var venda = _db.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .Include(v => v.Usuario)
                .FirstOrDefault(v => v.VendaID == id);

            if (venda == null)
                return NotFound();

            // busca a descrição do cancelamento se existir
            var motivoCancelamento = _db.MovimentacoesEstoque
                .Include(m => m.Usuario) // 👈 IMPORTANTE
                .Where(m => m.ItemVenda.VendaID == id
                         && m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Cancelamento
                         && m.MovimentacaoDescricao != null)
                .Select(m => new
                {
                    m.MovimentacaoDescricao,
                    UsuarioNome = m.Usuario.UsuarioNome
                })
                .FirstOrDefault();

            ViewBag.MotivoCancelamento = motivoCancelamento?.MovimentacaoDescricao;
            ViewBag.UsuarioCancelamento = motivoCancelamento?.UsuarioNome;

            return View(venda);
        }

        // Ação para exibir o formulário de cadastro de venda, incluindo a lista de produtos ativos para seleção
        public IActionResult Cadastrar()
        {
            var produtos = _db.Produtos
                .Where(p => p.ProdutoAtivo)
                .ToList();

            ViewBag.Produtos = produtos;

            return View();
        }

        [HttpPost]
        public IActionResult FinalizarVenda(VendaModel venda)
        {
            

            if (venda == null || venda.Itens.Count == 0)
            {
                TempData["MensagemErro"] = "Adicione pelo menos um item à venda.";
                ViewBag.Produtos = _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
                return View("Cadastrar", venda);
            }

            if (string.IsNullOrEmpty(venda.VendaTipoPagamento))
            {
                TempData["MensagemErro"] = "Selecione um tipo de pagamento.";
                ViewBag.Produtos = _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
                return View("Cadastrar", venda);
            }

            using var transaction = _db.Database.BeginTransaction();

            try
            {
                // busca todos os produtos necessários de uma vez só
                var ids = venda.Itens.Select(i => i.ProdutoID).ToList();
                var produtos = _db.Produtos
                    .Where(p => ids.Contains(p.ProdutoID))
                    .ToList();

                decimal totalVenda = 0;

                foreach (var item in venda.Itens)
                {
                    var produto = produtos.FirstOrDefault(p => p.ProdutoID == item.ProdutoID);

                    if (produto == null)
                    {
                        TempData["MensagemErro"] = $"Produto não encontrado: ID {item.ProdutoID}";
                        transaction.Rollback();
                        ViewBag.Produtos = _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
                        return View("Cadastrar", venda);
                    }

                    if ((produto.ProdutoQtdEstoque ?? 0) < item.ItemVendaQtd)
                    {
                        TempData["MensagemErro"] = $"Estoque insuficiente para: {produto.ProdutoNome}";
                        transaction.Rollback();
                        ViewBag.Produtos = _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
                        return View("Cadastrar", venda);
                    }

                    decimal precoBase = produto.ProdutoPrecoVenda ?? 0;
                    decimal desconto = produto.ProdutoPromocao;
                    decimal precoFinal = desconto > 0
                        ? precoBase - (precoBase * (desconto / 100))
                        : precoBase;

                    item.ItemVendaPreco = precoFinal;
                    item.ItemVendaTotal = precoFinal * item.ItemVendaQtd;
                    totalVenda += item.ItemVendaTotal;
                }

                var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

                venda.VendaValorTotal = totalVenda;
                venda.UsuarioID = usuarioId;

                _db.Vendas.Add(venda);
                _db.SaveChanges();

                
                foreach (var item in venda.Itens)
                {
                    var produto = produtos.First(p => p.ProdutoID == item.ProdutoID);
                    produto.ProdutoQtdEstoque -= item.ItemVendaQtd;

                    var movimentacao = new MovimentacaoEstoqueModel
                    {
                        ProdutoID = produto.ProdutoID,
                        MovimentacaoQtd = item.ItemVendaQtd,
                        MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Saida,
                        MovimentacaoData = DateTime.Now,
                        ItemVendaID = item.ItemVendaID,
                        UsuarioID = usuarioId
                    };

                    _db.MovimentacoesEstoque.Add(movimentacao);
                }

                _db.SaveChanges();
                transaction.Commit();

                TempData["MensagemSucesso"] = "Venda realizada com sucesso!";
                return RedirectToAction("Cadastrar");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["MensagemErro"] = $"Erro ao salvar venda: {ex.Message}";
                ViewBag.Produtos = _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
                return View("Cadastrar", venda);
            }
        }
        [AdminFilter]
        public IActionResult Cancelar(int id)
        {
            var venda = _db.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .Include(v => v.Usuario)
                .FirstOrDefault(v => v.VendaID == id);

            if (venda == null)
                return NotFound();

            return View(venda);
        }
        [AdminFilter]
        [HttpPost]
        public IActionResult CancelarVenda(int id, string? descricao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            var venda = _db.Vendas
                .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefault(v => v.VendaID == id);

            if (venda == null)
                return NotFound();

            if (DateTime.Now > venda.VendaData.AddHours(24))
            {
                TempData["MensagemErro"] = "Não é possível cancelar a venda após 24 horas.";
                return RedirectToAction("Index");
            }

            if (venda.VendaCancelada)
            {
                TempData["MensagemErro"] = "Venda já está cancelada.";
                return RedirectToAction("Index");
            }

            var produtos = _db.Produtos
                .Where(p => venda.Itens.Select(i => i.ProdutoID).Contains(p.ProdutoID))
                .ToList();

            foreach (var item in venda.Itens)
            {
                var produto = produtos.First(p => p.ProdutoID == item.ProdutoID);

                produto.ProdutoQtdEstoque += item.ItemVendaQtd;

                _db.MovimentacoesEstoque.Add(new MovimentacaoEstoqueModel
                {
                    ProdutoID = produto.ProdutoID,
                    MovimentacaoQtd = item.ItemVendaQtd,
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Cancelamento,
                    MovimentacaoData = DateTime.Now,
                    ItemVendaID = item.ItemVendaID,
                    MovimentacaoDescricao = descricao,
                    UsuarioID = usuarioId,
                });
            }

            venda.VendaCancelada = true;
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Venda cancelada com sucesso!";
            return RedirectToAction("Index");
        }
    }
}