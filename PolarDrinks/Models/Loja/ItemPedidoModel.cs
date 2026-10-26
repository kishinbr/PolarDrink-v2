using System.ComponentModel.DataAnnotations;
using PolarDrinks.Models;

namespace PolarDrinks.Models.Loja
{
    public class ItemPedidoModel
    {
        [Key]
        public int ItemPedidoID { get; set; }

        public int PedidoID { get; set; }
        public PedidoModel? Pedido { get; set; }

        public int ProdutoID { get; set; }
        public ProdutoModel? Produto { get; set; }

        public int ItemPedidoQtd { get; set; }

        public decimal ItemPedidoPreco { get; set; }

        public decimal ItemPedidoCusto { get; set; }
    }
}
