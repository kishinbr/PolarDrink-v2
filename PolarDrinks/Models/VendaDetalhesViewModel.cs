using PolarDrinks.Models;

namespace PolarDrinks.Models
{
    public class VendaDetalhesViewModel
    {
        public VendaModel Venda { get; set; } = null!;
        public string? MotivoCancelamento { get; set; }
        public string? UsuarioCancelamento { get; set; }
    }
}