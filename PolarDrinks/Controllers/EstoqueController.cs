using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Filters;
using PolarDrinks.Models;

namespace PolarDrinks.Controllers
{
    [AuthFilter]
    public class EstoqueController : Controller
    {
        // Injeção do contexto para acessar o banco de dados
        readonly ApplicationDbContext _db;

        public EstoqueController(ApplicationDbContext db)
        {
            _db = db;
        }
        // Ação para exibir a lista de produtos
        public IActionResult Index()
        {
            var produtos = _db.Produtos.ToList();
            return View(produtos);
        }
        [AdminFilter]
        // Ação para exibir o formulário de cadastro de produto
        public IActionResult Cadastrar()
        {
            return View();
        }

        // Ação para processar o formulário de cadastro de produto
        [HttpPost]
        [AdminFilter]
        public IActionResult Cadastrar(ProdutoModel produto)
        {

            bool codigoExiste = _db.Produtos
                .Any(x => x.ProdutoCodBarra == produto.ProdutoCodBarra);

            if (codigoExiste)
            {
                ModelState.AddModelError("ProdutoCodBarra", "Este código de barras já está cadastrado.");
            }


            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Erro ao cadastrar produto.";
                return View(produto);
            }


            _db.Produtos.Add(produto);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Produto cadastrado com sucesso!";
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

            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoID == id);
            if (produto == null) return NotFound();

            return View(produto);
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult Editar(ProdutoModel produto)
        {
            //verificar se o modelo é válido
            bool codigoExiste = _db.Produtos
            .Any(x => x.ProdutoCodBarra == produto.ProdutoCodBarra
                   && x.ProdutoID != produto.ProdutoID);

            if (codigoExiste)
            {
                ModelState.AddModelError("ProdutoCodBarra", "Este código de barras já está cadastrado.");
            }
            if (!ModelState.IsValid)
            {
                //caso contrário, retornar para a view com os erros de validação
                var produtoOriginal = _db.Produtos
                    .FirstOrDefault(p => p.ProdutoID == produto.ProdutoID);

                return View(produtoOriginal);
            }

            //caso seja válido, atualizar o produto no banco de dados
            var produtoDb = _db.Produtos.FirstOrDefault(p => p.ProdutoID == produto.ProdutoID);
            if (produtoDb == null) return NotFound();


            produtoDb.ProdutoNome = produto.ProdutoNome;
            produtoDb.ProdutoDescricao = produto.ProdutoDescricao;
            produtoDb.ProdutoCodBarra = produto.ProdutoCodBarra;
            produtoDb.ProdutoPrecoVenda = produto.ProdutoPrecoVenda;
            produtoDb.ProdutoAtivo = produto.ProdutoAtivo;
            produtoDb.ProdutoEstoqueMinimo = produto.ProdutoEstoqueMinimo;
            produtoDb.ProdutoPrecoCusto = produto.ProdutoPrecoCusto;
            produtoDb.ProdutoPromocao = produto.ProdutoPromocao;
            produtoDb.ProdutoQtdEstoque = produto.ProdutoQtdEstoque;


            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Produto atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult EdicaoRapida(int ProdutoID, decimal? ProdutoPrecoVenda, decimal? ProdutoPromocao)
        {
            // Verificar se os dados são válidos
            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Valores inválidos!";
                return RedirectToAction("Index");
            }

            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoID == ProdutoID);

            if (produto == null)
            {
                TempData["MensagemErro"] = "Produto não encontrado!";
                return RedirectToAction("Index");
            }

            if (ProdutoPrecoVenda == null || ProdutoPrecoVenda < 0)
            {
                TempData["MensagemErro"] = "Preço inválido!";
                return RedirectToAction("Index");
            }

            if (ProdutoPromocao == null || ProdutoPromocao < 0 || ProdutoPromocao > 100)
            {
                TempData["MensagemErro"] = "Promoção inválida!";
                return RedirectToAction("Index");
            }

            // Atualizar os campos do produto
            produto.ProdutoPrecoVenda = ProdutoPrecoVenda.Value;
            produto.ProdutoPromocao = ProdutoPromocao ?? 0;

            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Produto atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AdminFilter]
        public IActionResult AjustarEstoque(int ProdutoID, int NovaQuantidade, string Descricao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");

            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoID == ProdutoID);
            if (produto == null) return NotFound();

            // Calcular a diferença entre a nova quantidade e a quantidade atual em estoque
            int quantidadeAntiga = produto.ProdutoQtdEstoque ?? 0;
            int diferenca = NovaQuantidade - quantidadeAntiga;

            // Registrar a movimentação de estoque apenas se houver uma diferença
            if (diferenca != 0)
            {
                var movimentacao = new MovimentacaoEstoqueModel
                {
                    ProdutoID = produto.ProdutoID,

                    MovimentacaoQtd = diferenca,

                    MovimentacaoData = DateTime.Now,
                    MovimentacaoTipo = MovimentacaoEstoqueModel.Tipos.Edicao,
                    MovimentacaoDescricao = Descricao,
                    UsuarioID = usuarioId,
                };

                _db.MovimentacoesEstoque.Add(movimentacao);
            }
            // Atualizar a quantidade em estoque do produto
            produto.ProdutoQtdEstoque = NovaQuantidade;

            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Estoque ajustado com sucesso!";
            return RedirectToAction("Editar", new { id = ProdutoID });
        }


        // Ação para exibir as movimentações de estoque de um produto
        [AdminFilter]
        public IActionResult Movimentacoes(int? produtoId)
        {
            // Obter a lista de produtos para exibir no dropdown
            var produtos = _db.Produtos
                .Where(p => p.ProdutoAtivo)
                .ToList();
            // Passar a lista de produtos para a view
            ViewBag.Produtos = produtos;

            // Se nenhum produto for selecionado, exibir uma lista vazia
            if (produtoId == null)
            {
                return View(new List<MovimentacaoEstoqueModel>());
            }
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoID == produtoId);

            if (produto == null)
            {
                return View(new List<MovimentacaoEstoqueModel>());
            }
            // Obter as movimentações de estoque do produto selecionado, ordenadas pela data mais recente
            var movimentacoes = _db.MovimentacoesEstoque
                .Where(m => m.ProdutoID == produtoId)
                .Include(m => m.ItemVenda)
                    .ThenInclude(iv => iv.Venda)
                .Include(m => m.ItemCompra)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.MovimentacaoData)
                .ToList();

            ViewBag.ProdutoSelecionado = produto;

            return View(movimentacoes);
        }
    }
}