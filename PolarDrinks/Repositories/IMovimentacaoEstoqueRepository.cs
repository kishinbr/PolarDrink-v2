using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IMovimentacaoEstoqueRepository
    {
        void Adicionar(MovimentacaoEstoqueModel movimentacao);
        List<MovimentacaoEstoqueModel> ObterPorProduto(int produtoId);
    }
}