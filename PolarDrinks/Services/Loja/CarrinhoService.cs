using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories.Loja;

namespace PolarDrinks.Services.Loja
{
    public class CarrinhoService : ICarrinhoService
    {
        private readonly ICarrinhoRepository _carrinhoRepository;

        public CarrinhoService(ICarrinhoRepository carrinhoRepository)
        {
            _carrinhoRepository = carrinhoRepository;
        }

        public CarrinhoDto ObterCarrinho(int clienteId)
        {
            var itens = _carrinhoRepository.ObterItensDoCliente(clienteId);
            var carrinho = new CarrinhoDto();

            foreach (var item in itens)
            {
                if (item.Produto == null || !item.Produto.ProdutoAtivo)
                {
                    carrinho.AvisosRemocao.Add($"O produto '{item.Produto?.ProdutoNome ?? "desconhecido"}' não está mais disponível e foi removido do seu carrinho.");
                    _carrinhoRepository.Remover(item);
                    continue;
                }

                var precoBase = item.Produto.ProdutoPrecoVenda ?? 0;
                var desconto = item.Produto.ProdutoPromocao;
                var precoFinal = desconto > 0 ? precoBase - (precoBase * (desconto / 100)) : precoBase;

                carrinho.Itens.Add(new CarrinhoItemDto
                {
                    ProdutoID = item.ProdutoID,
                    ProdutoNome = item.Produto.ProdutoNome,
                    ProdutoImagemUrl = item.Produto.ProdutoImagemUrl,
                    PrecoUnitario = precoFinal,
                    Quantidade = item.Quantidade
                });
            }

            _carrinhoRepository.SalvarAlteracoes();

            return carrinho;
        }
        public void AdicionarItem(int clienteId, int produtoId, int quantidade)
        {
            var itemExistente = _carrinhoRepository.ObterItem(clienteId, produtoId);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade;
            }
            else
            {
                _carrinhoRepository.Adicionar(new CarrinhoItemModel
                {
                    ClienteID = clienteId,
                    ProdutoID = produtoId,
                    Quantidade = quantidade,
                    AdicionadoEm = DateTime.Now
                });
            }

            _carrinhoRepository.SalvarAlteracoes();
        }

        public void AtualizarQuantidade(int clienteId, int produtoId, int novaQuantidade)
        {
            var item = _carrinhoRepository.ObterItem(clienteId, produtoId);
            if (item == null) return;

            if (novaQuantidade <= 0)
            {
                _carrinhoRepository.Remover(item);
            }
            else
            {
                item.Quantidade = novaQuantidade;
            }

            _carrinhoRepository.SalvarAlteracoes();
        }

        public void RemoverItem(int clienteId, int produtoId)
        {
            var item = _carrinhoRepository.ObterItem(clienteId, produtoId);
            if (item == null) return;

            _carrinhoRepository.Remover(item);
            _carrinhoRepository.SalvarAlteracoes();
        }

        public void LimparCarrinho(int clienteId)
        {
            _carrinhoRepository.RemoverTodos(clienteId);
            _carrinhoRepository.SalvarAlteracoes();
        }
        public void MesclarCarrinho(int clienteId, List<ItemMesclagemDto> itensLocalStorage)
        {
            foreach (var itemLocal in itensLocalStorage)
            {
                AdicionarItem(clienteId, itemLocal.ProdutoID, itemLocal.Quantidade);
            }
        }

    }
}