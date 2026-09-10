using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models.Loja
{
    public class CarrinhoItemModel
    {
        [Key]
        public int CarrinhoItemID { get; set; }

        public int ClienteID { get; set; }
        public ClienteModel? Cliente { get; set; }

        public int ProdutoID { get; set; }
        public ProdutoModel? Produto { get; set; }

        public int Quantidade { get; set; }

        public DateTime AdicionadoEm { get; set; } = DateTime.Now;
    }
}