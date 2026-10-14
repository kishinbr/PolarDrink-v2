using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Services
{
    public interface ICarrinhoService
    {
        CarrinhoDto ObterCarrinho(int clienteId);
        ResultadoOperacao AdicionarItem(int clienteId, int produtoId, int quantidade);
        ResultadoOperacao AtualizarQuantidade(int clienteId, int produtoId, int novaQuantidade);
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