using PolarDrinks.Data;
using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public class FornecedorRepository : IFornecedorRepository
    {
        private readonly ApplicationDbContext _db;

        public FornecedorRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<FornecedorModel> ObterTodos()
        {
            return _db.Fornecedores.ToList();
        }

        public FornecedorModel? ObterPorId(int id)
        {
            return _db.Fornecedores.FirstOrDefault(x => x.FornecedorID == id);
        }

        public bool ExisteCNPJ(string cnpj, int? idParaIgnorar = null)
        {
            return _db.Fornecedores.Any(x =>
                x.FornecedorCNPJ == cnpj &&
                (idParaIgnorar == null || x.FornecedorID != idParaIgnorar));
        }

        public void Adicionar(FornecedorModel fornecedor)
        {
            _db.Fornecedores.Add(fornecedor);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}