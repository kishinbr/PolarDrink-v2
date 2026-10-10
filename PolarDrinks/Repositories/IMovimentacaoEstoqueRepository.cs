using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IMovimentacaoEstoqueRepository
    {
        void Adicionar(MovimentacaoEstoqueModel movimentacao);
        List<MovimentacaoEstoqueModel> ObterPorProduto(int produtoId);
        bool ExisteMovimentacao(string tipo, int? itemVendaId, int? itemPedidoId);
    }
}