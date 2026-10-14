using PolarDrinks.Compartilhado.Common;

namespace PolarDrinks.Modules.Estoque.Services
{
    public interface IMovimentacaoEstoqueService
    {
        ResultadoOperacao RegistrarSaida(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            int? itemVendaId = null, int? itemPedidoId = null);
        ResultadoOperacao RegistrarDevolucao(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            string? descricao = null, int? itemVendaId = null, int? itemPedidoId = null);
        ResultadoOperacao RegistrarEntrada(
            int produtoId, int quantidade, int? usuarioId, int itemCompraId);
    }
}