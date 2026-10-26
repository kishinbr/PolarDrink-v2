using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models.Loja
{
    public class ClienteModel
    {
        [Key]
        public int ClienteID { get; set; }

        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(100)]
        public string ClienteNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [StringLength(150)]
        public string ClienteEmail { get; set; } = string.Empty;

        public string ClienteSenhaHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone.")]
        [StringLength(20)]
        public string ClienteTelefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CPF.")]
        [StringLength(14)]
        public string ClienteCPF { get; set; } = string.Empty;

        public DateTime ClienteCriadoEm { get; set; } = DateTime.Now;

        public bool ClienteAtivo { get; set; } = true;
    }
}