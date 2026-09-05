using PolarDrinks.Data;
using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly ApplicationDbContext _db;

        public ProdutoRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<ProdutoModel> ObterTodos()
        {
            return _db.Produtos.ToList();
        }

        public List<ProdutoModel> ObterAtivos()
        {
            return _db.Produtos.Where(p => p.ProdutoAtivo).ToList();
        }

        public ProdutoModel? ObterPorId(int id)
        {
            return _db.Produtos.FirstOrDefault(p => p.ProdutoID == id);
        }

        public bool ExisteCodigoBarra(string codigoBarra, int? idParaIgnorar = null)
        {
            return _db.Produtos.Any(p =>
                p.ProdutoCodBarra == codigoBarra &&
                (idParaIgnorar == null || p.ProdutoID != idParaIgnorar));
        }

        public void Adicionar(ProdutoModel produto)
        {
            _db.Produtos.Add(produto);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}