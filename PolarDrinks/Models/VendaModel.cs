using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models
{
   
    public class VendaModel
    {
        [Key]
        public int VendaID { get; set; }

        public decimal VendaValorTotal { get; set; }

        [Required]
        [StringLength(30)]
        public string VendaTipoPagamento { get; set; }

        public DateTime VendaData { get; set; } = DateTime.Now;

        public bool VendaCancelada { get; set; }

        public int? UsuarioID { get; set; }
        public UsuarioModel? Usuario { get; set; }

        public List<ItemVendaModel> Itens { get; set; } = new List<ItemVendaModel>();
    }
    
}
