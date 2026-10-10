using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public class MovimentacaoEstoqueService : IMovimentacaoEstoqueService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;

        public MovimentacaoEstoqueService(
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository)
        {
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        public ResultadoOperacao RegistrarSaida(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            int? itemVendaId = null, int? itemPedidoId = null)
        {
            if (quantidade <= 0)
            {
                return ResultadoOperacao.Erro("A quantidade deve ser maior que zero.");
            }

            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null)
            {
                return ResultadoOperacao.Erro($"Produto não encontrado: ID {produtoId}");
            }

            if ((produto.ProdutoQtdEstoque ?? 0) < quantidade)
            {
                return ResultadoOperacao.Erro($"Estoque insuficiente para: {produto.ProdutoNome}");
            }

            produto.ProdutoQtdEstoque -= quantidade;

            _movimentacaoRepository.Adicionar(new MovimentacaoEstoqueModel
            {
                ProdutoID = produto.ProdutoID,
                MovimentacaoQtd = quantidade,
                MovimentacaoTipo = tipo,
                MovimentacaoData = DateTime.Now,
                ItemVendaID = itemVendaId,
                ItemPedidoID = itemPedidoId,
                UsuarioID = usuarioId
            });

            return ResultadoOperacao.Ok("Saída de estoque registrada.");
        }

        public ResultadoOperacao RegistrarDevolucao(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            string? descricao = null, int? itemVendaId = null, int? itemPedidoId = null)
        {
            if (tipo != MovimentacaoEstoqueModel.Tipos.Cancelamento
                && tipo != MovimentacaoEstoqueModel.Tipos.CancelamentoOnline)
            {
                return ResultadoOperacao.Erro("Tipo de devolução inválido.");
            }

            if (quantidade <= 0)
            {
                return ResultadoOperacao.Erro("A quantidade deve ser maior que zero.");
            }

            // Rastreabilidade: exatamente uma origem (item de venda OU item de pedido)
            if ((itemVendaId == null) == (itemPedidoId == null))
            {
                return ResultadoOperacao.Erro("Informe a origem da devolução (item de venda ou item de pedido).");
            }

            // A mesma origem não pode devolver o saldo duas vezes
            if (_movimentacaoRepository.ExisteMovimentacao(tipo, itemVendaId, itemPedidoId))
            {
                return ResultadoOperacao.Erro("Este item já foi devolvido ao estoque.");
            }

            var produto = _produtoRepository.ObterPorId(produtoId);
            if (produto == null)
            {
                return ResultadoOperacao.Erro($"Produto não encontrado: ID {produtoId}");
            }

            produto.ProdutoQtdEstoque = (produto.ProdutoQtdEstoque ?? 0) + quantidade;

            _movimentacaoRepository.Adicionar(new MovimentacaoEstoqueModel
            {
                ProdutoID = produto.ProdutoID,
                MovimentacaoQtd = quantidade,
                MovimentacaoTipo = tipo,
                MovimentacaoData = DateTime.Now,
                ItemVendaID = itemVendaId,
                ItemPedidoID = itemPedidoId,
                MovimentacaoDescricao = descricao,
                UsuarioID = usuarioId
            });

            return ResultadoOperacao.Ok("Devolução ao estoque registrada.");
        }
    }
}