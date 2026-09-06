using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
{
    public class CarrinhoRepository : ICarrinhoRepository
    {
        private readonly ApplicationDbContext _db;

        public CarrinhoRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<CarrinhoItemModel> ObterItensDoCliente(int clienteId)
        {
            return _db.CarrinhoItens
                .Include(ci => ci.Produto)
                .Where(ci => ci.ClienteID == clienteId)
                .ToList();
        }

        public CarrinhoItemModel? ObterItem(int clienteId, int produtoId)
        {
            return _db.CarrinhoItens
                .FirstOrDefault(ci => ci.ClienteID == clienteId && ci.ProdutoID == produtoId);
        }

        public void Adicionar(CarrinhoItemModel item)
        {
            _db.CarrinhoItens.Add(item);
        }

        public void Remover(CarrinhoItemModel item)
        {
            _db.CarrinhoItens.Remove(item);
        }

        public void RemoverTodos(int clienteId)
        {
            var itens = _db.CarrinhoItens.Where(ci => ci.ClienteID == clienteId);
            _db.CarrinhoItens.RemoveRange(itens);
        }

        public void SalvarAlteracoes()
        {
            _db.SaveChanges();
        }
    }
}