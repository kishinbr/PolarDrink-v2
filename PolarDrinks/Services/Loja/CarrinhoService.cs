using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;        
using PolarDrinks.Repositories.Loja;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services.Loja
{
    public class CarrinhoService : ICarrinhoService
    {
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IProdutoRepository _produtoRepository;

        public CarrinhoService(ICarrinhoRepository carrinhoRepository, IProdutoRepository produtoRepository)
        {
            _carrinhoRepository = carrinhoRepository;
            _produtoRepository = produtoRepository;
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
        public ResultadoOperacao AdicionarItem(int clienteId, int produtoId, int quantidade)
        {
            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null || !produto.ProdutoAtivo)
            {
                return ResultadoOperacao.Erro("Produto não encontrado ou indisponível.");
            }

            var itemExistente = _carrinhoRepository.ObterItem(clienteId, produtoId);
            var quantidadeTotal = (itemExistente?.Quantidade ?? 0) + quantidade;

            var disponivelOnline = Math.Max(0, (produto.ProdutoQtdEstoque ?? 0) - (produto.ProdutoEstoqueMinimo ?? 0));

            if (quantidadeTotal > disponivelOnline)
            {
                return disponivelOnline == 0
                    ? ResultadoOperacao.Erro($"{produto.ProdutoNome} apenas na loja física.")
                    : ResultadoOperacao.Erro($"Apenas {disponivelOnline} unidade(s) de {produto.ProdutoNome} disponíveis.");
            }

            if (itemExistente != null)
            {
                itemExistente.Quantidade = quantidadeTotal;
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
            return ResultadoOperacao.Ok();
        }

        public ResultadoOperacao AtualizarQuantidade(int clienteId, int produtoId, int novaQuantidade)
        {
            var item = _carrinhoRepository.ObterItem(clienteId, produtoId);
            if (item == null) return ResultadoOperacao.Erro("Item não encontrado no carrinho.");

            if (novaQuantidade <= 0)
            {
                _carrinhoRepository.Remover(item);
                _carrinhoRepository.SalvarAlteracoes();
                return ResultadoOperacao.Ok();
            }

            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null || !produto.ProdutoAtivo)
            {
                return ResultadoOperacao.Erro("Produto não encontrado ou indisponível.");
            }

            var disponivelOnline = Math.Max(0, (produto.ProdutoQtdEstoque ?? 0) - (produto.ProdutoEstoqueMinimo ?? 0));

            if (novaQuantidade > disponivelOnline)
            {
                return disponivelOnline == 0
                    ? ResultadoOperacao.Erro($"{produto.ProdutoNome} apenas na loja física.")
                    : ResultadoOperacao.Erro($"Apenas {disponivelOnline} unidade(s) de {produto.ProdutoNome} disponíveis.");
            }

            item.Quantidade = novaQuantidade;
            _carrinhoRepository.SalvarAlteracoes();
            return ResultadoOperacao.Ok();
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