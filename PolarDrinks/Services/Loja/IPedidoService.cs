using PolarDrinks.Models.Loja;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services.Loja
{
    public interface IPedidoService
    {
        ResultadoOperacao<PedidoModel> Checkout(int clienteId);
        List<PedidoModel> ListarPedidosDoCliente(int clienteId);
        PedidoModel? ObterDetalhePedido(int clienteId, int pedidoId);
        ResultadoOperacao CancelarPeloCliente(int clienteId, int pedidoId);
    }
}