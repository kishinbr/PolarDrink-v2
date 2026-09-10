using PolarDrinks.Models.Loja;

namespace PolarDrinks.Services.Loja
{
    public interface ICarrinhoService
    {
        CarrinhoDto ObterCarrinho(int clienteId);
        void AdicionarItem(int clienteId, int produtoId, int quantidade);
        void AtualizarQuantidade(int clienteId, int produtoId, int novaQuantidade);
        void RemoverItem(int clienteId, int produtoId);
        void LimparCarrinho(int clienteId);
        void MesclarCarrinho(int clienteId, List<ItemMesclagemDto> itensLocalStorage);
    }

    public class ItemMesclagemDto
    {
        public int ProdutoID { get; set; }
        public int Quantidade { get; set; }
    }
}