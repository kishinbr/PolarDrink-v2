using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
    {
        private readonly ApplicationDbContext _db;

        public MovimentacaoEstoqueRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public void Adicionar(MovimentacaoEstoqueModel movimentacao)
        {
            _db.MovimentacoesEstoque.Add(movimentacao);
        }

        public List<MovimentacaoEstoqueModel> ObterPorProduto(int produtoId)
        {
            return _db.MovimentacoesEstoque
                .Where(m => m.ProdutoID == produtoId)
                .Include(m => m.ItemVenda)
                    .ThenInclude(iv => iv.Venda)
                .Include(m => m.ItemCompra)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.MovimentacaoData)
                .ToList();
        }
    }
}