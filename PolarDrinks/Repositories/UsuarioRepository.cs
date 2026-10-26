using PolarDrinks.Data;
using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _db;

        public UsuarioRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<UsuarioModel> ObterTodosOrdenados()
        {
            return _db.Usuarios.OrderBy(u => u.UsuarioLogin).ToList();
        }

        public UsuarioModel? ObterPorId(int id)
        {
            return _db.Usuarios.Find(id);
        }

        public UsuarioModel? ObterPorLogin(string login)
        {
            return _db.Usuarios.FirstOrDefault(u => u.UsuarioLogin == login);
        }

        public bool ExisteLogin(string login)
        {
            return _db.Usuarios.Any(u => u.UsuarioLogin == login);
        }

        public int ContarAdminsAtivos()
        {
            return _db.Usuarios.Count(u => u.UsuarioAtivo && u.UsuarioPerfil == "Admin");
        }

        public void Adicionar(UsuarioModel usuario)
        {
            _db.Usuarios.Add(usuario);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}