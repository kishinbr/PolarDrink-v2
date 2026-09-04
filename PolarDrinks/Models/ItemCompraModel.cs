using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolarDrinks.Models
{
    //representa cada item comprado individualmente, com a quantidade, preço e o produto relacionado
    public class ItemCompraModel
    {
        [Key]
        public int ItemCompraID { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int ItemCompraQtd { get; set; }


        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0, 999999.99, ErrorMessage = "O preço deve ser positivo")]
        public decimal ItemCompraPreco { get; set; }


        [Required(ErrorMessage = "Selecione um produto")]
        public int ProdutoID { get; set; }

        public ProdutoModel? Produto { get; set; }

        [Required]
        public int CompraID { get; set; }

        public CompraEstoqueModel? Compra { get; set; }

        [NotMapped]
        public decimal ValorTotal
        {
            get
            {
                return ItemCompraQtd * ItemCompraPreco;
            }
        }
    }
}