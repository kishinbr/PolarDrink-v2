namespace PolarDrinks.Models.Loja
{
    public class CatalogoProdutoDto
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string? ProdutoDescricao { get; set; }
        public decimal ProdutoPrecoVenda { get; set; }
        public decimal ProdutoPromocao { get; set; }
        public string? ProdutoImagemUrl { get; set; }
        public int ProdutoQtdEstoque { get; set; }
        public List<string> Categorias { get; set; } = new List<string>();
    }
}