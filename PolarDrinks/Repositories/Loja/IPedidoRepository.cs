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

        List<PedidoModel> ObterPorStatus(string status);
        List<PedidoModel> ObterTodosComDetalhes();
        (int Quantidade, decimal Faturamento, decimal Lucro) ObterResumoFinanceiro(
            IEnumerable<string> statusValidos, DateTime? inicio, DateTime? fim);
    }
}