using Microsoft.AspNetCore.Mvc.Rendering;
using PolarDrinks.Models;
using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.ViewModels
{
    //essa classe é usada para criar uma nova compra, ela tem as propriedades necessárias para preencher o formulário de criação de compra,
    //incluindo a lista de itens e as opções de fornecedores e produtos
    public class CompraCreateViewModel
    {
       
        [Required(ErrorMessage = "Selecione um fornecedor")]
        public int? FornecedorID { get; set; }

        //essa lista representa os itens que o usuário vai adicionar na compra, cada item tem o produto selecionado, a quantidade e o preço
        public List<ItemCompraCreateVM> Itens { get; set; } = new List<ItemCompraCreateVM>();

        public IEnumerable<SelectListItem>? Fornecedores { get; set; }
        public IEnumerable<SelectListItem>? Produtos { get; set; }
        public List<ProdutoModel> ProdutosEstoque { get; set; } = new List<ProdutoModel>();
    }

    //essa classe representa cada item que o usuário vai adicionar na compra, ela tem as propriedades necessárias para preencher o formulário de cada item
    public class ItemCompraCreateVM
    {
        [Required(ErrorMessage = "Selecione um produto")]
        public int? ProdutoID { get; set; }
        public string? ProdutoNome { get; set; }

        [Required(ErrorMessage = "Informe a quantidade")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade inválida")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "Informe o preço")]
        [Range(0.01, 999999.99, ErrorMessage = "Preço inválido")]
        public decimal Preco { get; set; }
    }
}