using PolarDrinks.Models;

namespace PolarDrinks.ViewModels
{
    //essa classe é usada para exibir os detalhes de uma compra, ela tem as propriedades necessárias para mostrar as informações da compra,
    //como o fornecedor, a data, o status e a lista de itens comprados
    public class CompraDetalhesViewModel
    {
        public CompraEstoqueModel Compra { get; set; }

        public bool PodeConfirmar { get; set; }
        public List<ItemCompraModel> Itens { get; set; }
    }
}