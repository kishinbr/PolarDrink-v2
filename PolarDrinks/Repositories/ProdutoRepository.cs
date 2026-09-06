using PolarDrinks.Data;
using PolarDrinks.Models;
using PolarDrinks.Models.Loja;

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
        public List<ProdutoModel> ObterPorIds(List<int> ids)
        {
            return _db.Produtos.Where(p => ids.Contains(p.ProdutoID)).ToList();
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
        public List<int> ObterCategoriasDoProduto(int produtoId)
        {
            return _db.ProdutoCategorias
                .Where(pc => pc.ProdutoID == produtoId)
                .Select(pc => pc.CategoriaID)
                .ToList();
        }

        public void DefinirCategoriasDoProduto(int produtoId, List<int> categoriaIds)
        {
            var atuais = _db.ProdutoCategorias.Where(pc => pc.ProdutoID == produtoId);
            _db.ProdutoCategorias.RemoveRange(atuais);

            foreach (var categoriaId in categoriaIds)
            {
                _db.ProdutoCategorias.Add(new ProdutoCategoriaModel
                {
                    ProdutoID = produtoId,
                    CategoriaID = categoriaId
                });
            }
        }
    }
}