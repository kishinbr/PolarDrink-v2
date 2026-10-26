namespace PolarDrinks.Models.Loja
{
    public class ProdutoCategoriaModel
    {
        public int ProdutoID { get; set; }
        public ProdutoModel? Produto { get; set; }

        public int CategoriaID { get; set; }
        public CategoriaModel? Categoria { get; set; }
    }
}
