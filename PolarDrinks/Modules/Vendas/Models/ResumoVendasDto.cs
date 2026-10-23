namespace PolarDrinks.Modules.Vendas.Models
{
    public class ResumoVendasDto
    {
        public int QuantidadeVendas { get; set; }
        public decimal Faturamento { get; set; }
        public decimal Lucro { get; set; }
        public decimal TotalDinheiro { get; set; }
        public decimal TotalPix { get; set; }
        public decimal TotalCartao { get; set; }
        public int QuantidadeCanceladas { get; set; }
        public decimal ValorCancelado { get; set; }
    }
}