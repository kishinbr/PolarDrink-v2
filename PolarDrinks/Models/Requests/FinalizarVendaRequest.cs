namespace PolarDrinks.Models.Requests
{
    /// <summary>
    /// Contrato de entrada de FinalizarVenda: contém SOMENTE o que o cliente pode informar.
    /// Preço, custo, total, data, usuário e status são definidos pelo servidor.
    /// </summary>
    public class FinalizarVendaRequest
    {
        public string? VendaTipoPagamento { get; set; }

        public List<ItemVendaRequest> Itens { get; set; } = new();
    }

    public class ItemVendaRequest
    {
        public int ProdutoID { get; set; }

        public int ItemVendaQtd { get; set; }
    }
}