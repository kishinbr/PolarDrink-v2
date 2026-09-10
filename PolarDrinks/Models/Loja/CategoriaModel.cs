using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models.Loja
{
    public class CategoriaModel
    {
        [Key]
        public int CategoriaID { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(50)]
        public string CategoriaNome { get; set; } = string.Empty;

        public bool CategoriaAtiva { get; set; } = true;
    }
}