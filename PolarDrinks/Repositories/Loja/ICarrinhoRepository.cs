using PolarDrinks.Models.Loja;

namespace PolarDrinks.Repositories.Loja
{
    public interface ICarrinhoRepository
    {
        List<CarrinhoItemModel> ObterItensDoCliente(int clienteId);
        CarrinhoItemModel? ObterItem(int clienteId, int produtoId);

        void Adicionar(CarrinhoItemModel item);
        void Remover(CarrinhoItemModel item);
        void RemoverTodos(int clienteId);
        void SalvarAlteracoes();
    }
}