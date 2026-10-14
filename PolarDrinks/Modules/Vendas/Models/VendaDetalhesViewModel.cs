namespace PolarDrinks.Modules.Vendas.Models
{
    public class VendaDetalhesViewModel
    {
        public VendaModel Venda { get; set; } = null!;
        public string? MotivoCancelamento { get; set; }
        public string? UsuarioCancelamento { get; set; }
    }
}

//pode excluir esse arquivo, não está sendo usado