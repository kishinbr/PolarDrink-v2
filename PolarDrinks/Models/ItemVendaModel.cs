using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models
{
    public class ItemVendaModel
    {
        [Key]
        public int ItemVendaID { get; set; }

        public int ItemVendaQtd { get; set; }

        public decimal ItemVendaPreco { get; set; }

        public decimal ItemVendaCusto { get; set; }
        public decimal ItemVendaTotal { get; set; } 

        public int ProdutoID { get; set; }
        public ProdutoModel? Produto { get; set; }

        public int VendaID { get; set; }
        public VendaModel? Venda { get; set; }
    }
}