using PolarDrinks.Data;
using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoriaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<CategoriaModel> ObterTodas()
        {
            return _db.Categorias.ToList();
        }

        public List<CategoriaModel> ObterAtivas()
        {
            return _db.Categorias.Where(c => c.CategoriaAtiva).ToList();
        }

        public CategoriaModel? ObterPorId(int id)
        {
            return _db.Categorias.FirstOrDefault(c => c.CategoriaID == id);
        }

        public bool ExisteNome(string nome, int? idParaIgnorar = null)
        {
            return _db.Categorias.Any(c =>
                c.CategoriaNome == nome &&
                (idParaIgnorar == null || c.CategoriaID != idParaIgnorar));
        }

        public void Adicionar(CategoriaModel categoria)
        {
            _db.Categorias.Add(categoria);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}