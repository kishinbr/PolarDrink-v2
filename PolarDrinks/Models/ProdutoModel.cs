using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolarDrinks.Models
{
    public class ProdutoModel
    {
        [Key]
        public int ProdutoID { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório")]
        [StringLength(100)]
        public string? ProdutoNome { get; set; }

        [StringLength(200)]
        public string? ProdutoDescricao { get; set; }

        [Required(ErrorMessage = "O custo é obrigatório")]
        [Range(0.00, 999999.99)]
        public decimal? ProdutoPrecoCusto { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.00, 999999.99, ErrorMessage = "O preço inválido")]
        public decimal? ProdutoPrecoVenda { get; set; }

        [Range(0, 100, ErrorMessage = "Promoção deve ser entre 0 e 100%")]
        public decimal ProdutoPromocao { get; set; } = 0; 

        [Required(ErrorMessage = "O código de barras é obrigatório")]
        [StringLength(20, MinimumLength = 13, ErrorMessage = "O código de barras deve ter 13 caracteres")]
        public string? ProdutoCodBarra { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantidade inválida")]
        public int? ProdutoQtdEstoque { get; set; }

        [Required(ErrorMessage = "O estoque mínimo é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "Estoque mínimo inválido")]
        public int? ProdutoEstoqueMinimo { get; set; } 

        public bool ProdutoAtivo { get; set; }

        public string? ProdutoImagemUrl { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        [NotMapped]
        public bool EstoqueBaixo => ProdutoQtdEstoque.HasValue && ProdutoQtdEstoque.Value <= ProdutoEstoqueMinimo;
        public decimal CalcularPrecoFinal()
        {
            var precoBase = Math.Max(0m, ProdutoPrecoVenda ?? 0m);

            var promocaoValida = ProdutoPromocao > 0 && ProdutoPromocao <= 100;

            var precoFinal = promocaoValida
                ? precoBase - (precoBase * (ProdutoPromocao / 100m))
                : precoBase;

            return Math.Round(precoFinal, 2, MidpointRounding.AwayFromZero);
        }
    }
}