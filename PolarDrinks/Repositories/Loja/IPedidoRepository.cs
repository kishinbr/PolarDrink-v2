using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
{
    public interface IPedidoRepository
    {
        void Adicionar(PedidoModel pedido);
        PedidoModel? ObterPorId(int pedidoId);
        PedidoModel? ObterPorIdEcliente(int pedidoId, int clienteId);
        List<PedidoModel> ObterPedidosDoCliente(int clienteId);
        bool ExisteCodigoAtivo(string codigo);
    }
}