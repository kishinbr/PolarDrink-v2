using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models
{
    public class UsuarioModel
    {
        [Key]
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "Informe o nome do usuário")]
        [StringLength(80)]
        public string UsuarioNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o login do usuário")]
        [StringLength(50)]
        public string UsuarioLogin { get; set; } = string.Empty;

        public string UsuarioSenhaHash { get; set; } = string.Empty;

        public string UsuarioPerfil { get; set; } = "Funcionario";

        public bool UsuarioAtivo { get; set; } = true;

        public DateTime UsuarioCriadoEm { get; set; } = DateTime.Now;
    }
}