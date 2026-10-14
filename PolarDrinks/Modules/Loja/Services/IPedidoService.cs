using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Services
{
    public interface IPedidoService
    {
        ResultadoOperacao<PedidoModel> Checkout(int clienteId, string tipoPagamento);
        List<PedidoModel> ListarPedidosDoCliente(int clienteId);
        PedidoModel? ObterDetalhePedido(int clienteId, int pedidoId);
        ResultadoOperacao CancelarPeloCliente(int clienteId, int pedidoId);
        ResultadoOperacao CancelarAposEntrega(int pedidoId, string descricao, int usuarioId);
        List<PedidoModel> ListarPorStatus(string status);
        PedidoModel? ObterPedidoAdmin(int pedidoId);
        ResultadoOperacao MarcarComoSeparado(int pedidoId, int usuarioId);
        ResultadoOperacao VoltarParaSeparacao(int pedidoId);
        ResultadoOperacao ConfirmarEntrega(int pedidoId, string codigoInformado, int usuarioId);
        int ExpirarPedidosNaoRetirados();
    }
}