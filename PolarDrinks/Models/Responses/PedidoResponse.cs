using PolarDrinks.Models.Loja;

namespace PolarDrinks.Models.Responses
{
    /// <summary>
    /// Contrato de resposta dos pedidos da loja online (API do cliente).
    /// Contém SOMENTE o que a loja precisa exibir. Não expõe custo, dados internos
    /// do produto, dados do cliente nem os funcionários que atenderam o pedido.
    /// O código de retirada faz parte do contrato: o cliente precisa dele para retirar
    /// o pedido, e os endpoints só devolvem pedidos do próprio cliente logado.
    /// </summary>
    public class PedidoResponse
    {
        public int PedidoID { get; set; }
        public string PedidoCodigo { get; set; } = string.Empty;
        public DateTime PedidoData { get; set; }
        public string PedidoStatus { get; set; } = string.Empty;
        public decimal PedidoValorTotal { get; set; }
        public string PedidoTipoPagamento { get; set; } = string.Empty;
        public DateTime? PedidoDataSeparado { get; set; }
        public DateTime? PedidoDataConcluido { get; set; }
        public List<ItemPedidoResponse> Itens { get; set; } = new();

        public static PedidoResponse De(PedidoModel pedido)
        {
            return new PedidoResponse
            {
                PedidoID = pedido.PedidoID,
                PedidoCodigo = pedido.PedidoCodigo,
                PedidoData = pedido.PedidoData,
                PedidoStatus = pedido.PedidoStatus,
                PedidoValorTotal = pedido.PedidoValorTotal,
                PedidoTipoPagamento = pedido.PedidoTipoPagamento,
                PedidoDataSeparado = pedido.PedidoDataSeparado,
                PedidoDataConcluido = pedido.PedidoDataConcluido,
                Itens = pedido.Itens.Select(ItemPedidoResponse.De).ToList()
            };
        }
    }

    public class ItemPedidoResponse
    {
        public int ItemPedidoQtd { get; set; }
        public decimal ItemPedidoPreco { get; set; }
        public ProdutoPedidoResponse? Produto { get; set; }

        public static ItemPedidoResponse De(ItemPedidoModel item)
        {
            return new ItemPedidoResponse
            {
                ItemPedidoQtd = item.ItemPedidoQtd,
                ItemPedidoPreco = item.ItemPedidoPreco,
                Produto = item.Produto == null
                    ? null
                    : new ProdutoPedidoResponse { ProdutoNome = item.Produto.ProdutoNome }
            };
        }
    }

    public class ProdutoPedidoResponse
    {
        public string? ProdutoNome { get; set; }
    }
}