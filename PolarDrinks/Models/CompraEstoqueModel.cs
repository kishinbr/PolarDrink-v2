using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolarDrinks.Models
{
    //representa a compra inteira, com seus dados e a lista de itens comprados
    public class CompraEstoqueModel
    {
        [Key]
        public int CompraID { get; set; }

        [Required]
        public DateTime CompraData { get; set; } = DateTime.Now;

        public DateTime? CompraDataEntrega { get; set; }

        [Required(ErrorMessage = "O status da compra é obrigatório")]
        [StringLength(20)]
        public string CompraStatus { get; set; } = "Aguardando";

        //essa propriedade não é mapeada para o banco, ela é calculada automatica somando o valor total de cada item da compra
        [NotMapped]
        public decimal CompraValorTotal
        {
            get
            {
                return Itens.Sum(i => i.ItemCompraQtd * i.ItemCompraPreco);
            }
        }

        [Required(ErrorMessage = "Selecione um fornecedor")]
        public int FornecedorID { get; set; }

        public FornecedorModel? Fornecedor { get; set; }

        public int? UsuarioID { get; set; }
        public UsuarioModel? Usuario { get; set; }

        //relacionamento 1:N com itens de compra

        //isso signiica que a compra tem uma lista de itens, da itemCompraModel, e que essa lista é inicializada como vazia para evitar null reference
        public ICollection<ItemCompraModel> Itens { get; set; } = new List<ItemCompraModel>();
    }
}