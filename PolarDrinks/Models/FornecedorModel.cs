using System.ComponentModel.DataAnnotations;

namespace PolarDrinks.Models
{
    public class FornecedorModel
    {
        [Key]
        public int FornecedorID { get; set; }

        [Required(ErrorMessage = "O nome do fornecedor é obrigatório")]
        [StringLength(80)]
        public string? FornecedorNome { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "CNPJ deve ter 18 caracteres")]
        public string? FornecedorCNPJ { get; set; }


        [StringLength(15, MinimumLength = 15, ErrorMessage = "O telefone deve ter 15 caracteres")]
        [Required(ErrorMessage = "O Telefone é obrigatório")]
        public string? FornecedorTelefone { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [StringLength(80)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? FornecedorEmail { get; set; }

        [Required(ErrorMessage = "O CEP é obrigatório")]
        [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
        public string? FornecedorCEP { get; set; }

        [Required(ErrorMessage = "O estado é obrigatório")]
        [StringLength(2)]
        public string? FornecedorEstado { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [StringLength(100)]
        public string? FornecedorCidade { get; set; }

        [Required(ErrorMessage = "O bairro é obrigatório")]
        [StringLength(100)]
        public string? FornecedorBairro { get; set; }

        [Required(ErrorMessage = "O logradouro é obrigatório")]
        [StringLength(100)]
        public string? FornecedorLogradouro { get; set; }

        [Required(ErrorMessage = "O número é obrigatório")]
        [StringLength(10)]
        public string? FornecedorNum { get; set; }

        public bool FornecedorAtivo { get; set; } = true;

        public List<CompraEstoqueModel> Compras { get; set; } = new List<CompraEstoqueModel>();
    }
}