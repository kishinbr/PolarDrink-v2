namespace PolarDrinks.Models.Loja
{
    public class CarrinhoItemDto
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string? ProdutoImagemUrl { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }

    public class CarrinhoDto
    {
        public List<CarrinhoItemDto> Itens { get; set; } = new List<CarrinhoItemDto>();
        public decimal Total => Itens.Sum(i => i.Subtotal);
        public List<string> AvisosRemocao { get; set; } = new List<string>();
    }
}