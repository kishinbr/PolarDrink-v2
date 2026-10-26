using PolarDrinks.Data;
using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _db;

        public ClienteRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public ClienteModel? ObterPorId(int id)
        {
            return _db.Clientes.FirstOrDefault(c => c.ClienteID == id);
        }

        public ClienteModel? ObterPorEmail(string email)
        {
            return _db.Clientes.FirstOrDefault(c => c.ClienteEmail == email);
        }

        public bool ExisteEmail(string email)
        {
            return _db.Clientes.Any(c => c.ClienteEmail == email);
        }

        public bool ExisteCPF(string cpf)
        {
            return _db.Clientes.Any(c => c.ClienteCPF == cpf);
        }

        public void Adicionar(ClienteModel cliente)
        {
            _db.Clientes.Add(cliente);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}