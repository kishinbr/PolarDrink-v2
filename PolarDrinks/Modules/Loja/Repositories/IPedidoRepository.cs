using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Repositories
{
    public interface IPedidoRepository
    {
        void Adicionar(PedidoModel pedido);
        PedidoModel? ObterPorId(int pedidoId);
        PedidoModel? ObterPorIdEcliente(int pedidoId, int clienteId);
        List<PedidoModel> ObterPedidosDoCliente(int clienteId);
        bool ExisteCodigoAtivo(string codigo);

        List<PedidoModel> ObterPorStatus(string status);
        List<PedidoModel> ObterTodosComDetalhes();
        (int Quantidade, decimal Faturamento, decimal Lucro) ObterResumoFinanceiro(
            IEnumerable<string> statusValidos, DateTime? inicio, DateTime? fim);
    }
}