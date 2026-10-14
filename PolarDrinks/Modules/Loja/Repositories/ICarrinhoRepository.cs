using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Repositories
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