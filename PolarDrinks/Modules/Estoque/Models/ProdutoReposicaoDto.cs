namespace PolarDrinks.Modules.Estoque.Models
{
    public class ProdutoReposicaoDto
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoCodBarra { get; set; } = string.Empty;
        public int ProdutoQtdEstoque { get; set; }
        public int ProdutoEstoqueMinimo { get; set; }
        public int QuantidadeFaltante { get; set; }
        public decimal ProdutoPrecoCusto { get; set; }
    }
}