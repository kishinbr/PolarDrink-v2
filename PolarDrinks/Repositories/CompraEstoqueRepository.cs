using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public class CompraEstoqueRepository : ICompraEstoqueRepository
    {
        private readonly ApplicationDbContext _db;

        public CompraEstoqueRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<CompraEstoqueModel> ObterPorStatus(string status)
        {
            return _db.ComprasEstoque
                .Include(c => c.Fornecedor)
                .Where(c => c.CompraStatus == status)
                .OrderByDescending(c => c.CompraData)
                .ToList();
        }

        public CompraEstoqueModel? ObterDetalhes(int id)
        {
            return _db.ComprasEstoque
                .Include(c => c.Fornecedor)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.CompraID == id);
        }

        public CompraEstoqueModel? ObterParaConfirmarEntrega(int id)
        {
            return _db.ComprasEstoque
                .Include(c => c.Itens)
                .FirstOrDefault(c => c.CompraID == id);
        }

        public string? ObterUsuarioConfirmacaoEntrada(int compraId)
        {
            return _db.MovimentacoesEstoque
                .Include(m => m.Usuario)
                .Where(m => m.ItemCompra.CompraID == compraId
                         && m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Entrada)
                .Select(m => m.Usuario.UsuarioNome)
                .FirstOrDefault();
        }

        public void Adicionar(CompraEstoqueModel compra)
        {
            _db.ComprasEstoque.Add(compra);
        }

        public void Remover(CompraEstoqueModel compra)
        {
            _db.ComprasEstoque.Remove(compra);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}