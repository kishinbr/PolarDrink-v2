using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IMovimentacaoEstoqueService
    {
        ResultadoOperacao RegistrarSaida(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            int? itemVendaId = null, int? itemPedidoId = null);
        ResultadoOperacao RegistrarDevolucao(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            string? descricao = null, int? itemVendaId = null, int? itemPedidoId = null);
    }
}