using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _db;

        public PedidoRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public void Adicionar(PedidoModel pedido)
        {
            _db.Pedidos.Add(pedido);
        }

        public PedidoModel? ObterPorId(int pedidoId)
        {
            return _db.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(p => p.Cliente)
                .FirstOrDefault(p => p.PedidoID == pedidoId);
        }

        public PedidoModel? ObterPorIdEcliente(int pedidoId, int clienteId)
        {
            return _db.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefault(p => p.PedidoID == pedidoId && p.ClienteID == clienteId);
        }

        public List<PedidoModel> ObterPedidosDoCliente(int clienteId)
        {
            return _db.Pedidos
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(p => p.ClienteID == clienteId)
                .OrderByDescending(p => p.PedidoData)
                .ToList();
        }

        public bool ExisteCodigoAtivo(string codigo)
        {
            var statusAtivos = new[] { PedidoModel.Status.AguardandoSeparacao, PedidoModel.Status.Separado };

            return _db.Pedidos.Any(p => p.PedidoCodigo == codigo && statusAtivos.Contains(p.PedidoStatus));
        }
    }
}