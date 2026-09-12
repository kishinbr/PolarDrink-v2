using Microsoft.EntityFrameworkCore;
using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;
using PolarDrinks.Repositories.Loja;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services.Loja
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            ICarrinhoRepository carrinhoRepository,
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            IUnitOfWork unitOfWork)
        {
            _pedidoRepository = pedidoRepository;
            _carrinhoRepository = carrinhoRepository;
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _unitOfWork = unitOfWork;
        }

        public List<PedidoModel> ListarPedidosDoCliente(int clienteId)
        {
            return _pedidoRepository.ObterPedidosDoCliente(clienteId);
        }
        public List<PedidoModel> ListarPorStatus(string status)
        {
            return _pedidoRepository.ObterPorStatus(status);
        }

        public PedidoModel? ObterPedidoAdmin(int pedidoId)
        {
            return _pedidoRepository.ObterPorId(pedidoId);
        }

        public PedidoModel? ObterDetalhePedido(int clienteId, int pedidoId)
        {
            return _pedidoRepository.ObterPorIdEcliente(pedidoId, clienteId);
        }

        private string GerarCodigoUnico()
        {
            var random = new Random();
            string codigo;

            do
            {
                codigo = random.Next(0, 10000).ToString("D4");
            }
            while (_pedidoRepository.ExisteCodigoAtivo(codigo));

            return codigo;
        }

        public ResultadoOperacao<PedidoModel> Checkout(int clienteId, string tipoPagamento)
        {
            if (tipoPagamento != PedidoModel.TipoPagamento.Cartao && tipoPagamento != PedidoModel.TipoPagamento.Pix)
            {
                return ResultadoOperacao<PedidoModel>.Erro("Forma de pagamento inválida.");
            }
            const int maxTentativas = 3;

            for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
            {
                var itensCarrinho = _carrinhoRepository.ObterItensDoCliente(clienteId);

                if (itensCarrinho.Count == 0)
                {
                    return ResultadoOperacao<PedidoModel>.Erro("Seu carrinho está vazio.");
                }

                _unitOfWork.BeginTransaction();

                try
                {
                    var pedido = new PedidoModel
                    {
                        PedidoCodigo = GerarCodigoUnico(),
                        ClienteID = clienteId,
                        PedidoData = DateTime.Now,
                        PedidoStatus = PedidoModel.Status.AguardandoSeparacao,
                        PedidoTipoPagamento = tipoPagamento,
                        Itens = new List<ItemPedidoModel>()
                    };

                    decimal totalPedido = 0;
                    bool estoqueInsuficiente = false;
                    string? mensagemEstoque = null;

                    foreach (var itemCarrinho in itensCarrinho)
                    {
                        var produto = _produtoRepository.ObterPorId(itemCarrinho.ProdutoID);

                        if (produto == null || !produto.ProdutoAtivo)
                        {
                            estoqueInsuficiente = true;
                            mensagemEstoque = $"Um produto do seu carrinho não está mais disponível.";
                            break;
                        }

                        if ((produto.ProdutoQtdEstoque ?? 0) < itemCarrinho.Quantidade)
                        {
                            estoqueInsuficiente = true;
                            mensagemEstoque = $"Estoque insuficiente para: {produto.ProdutoNome}";
                            break;
                        }

                        var precoBase = produto.ProdutoPrecoVenda ?? 0;
                        var desconto = produto.ProdutoPromocao;
                        var precoFinal = desconto > 0 ? precoBase - (precoBase * (desconto / 100)) : precoBase;

                        pedido.Itens.Add(new ItemPedidoModel
                        {
                            ProdutoID = produto.ProdutoID,
                            ItemPedidoQtd = itemCarrinho.Quantidade,
                            ItemPedidoPreco = precoFinal,
                            ItemPedidoCusto = produto.ProdutoPrecoCusto ?? 0
                        });

                        totalPedido += precoFinal * itemCarrinho.Quantidade;

                        produto.ProdutoQtdEstoque -= itemCarrinho.Quantidade;
                    }

                    if (estoqueInsuficiente)
                    {
                        _unitOfWork.Rollback();
                        return ResultadoOperacao<PedidoModel>.Erro(mensagemEstoque!);
                    }

                    pedido.PedidoValorTotal = totalPedido;

                    _pedidoRepository.Adicionar(pedido);
                    _unitOfWork.SaveChanges();

                    foreach (var item in pedido.Itens)
                    {
                        _movimentacaoRepository.Adicionar(new Models.MovimentacaoEstoqueModel
                        {
                            ProdutoID = item.ProdutoID,
                            MovimentacaoQtd = item.ItemPedidoQtd,
                            MovimentacaoTipo = Models.MovimentacaoEstoqueModel.Tipos.SaidaOnline,
                            MovimentacaoData = DateTime.Now,
                            ItemPedidoID = item.ItemPedidoID
                        });
                    }

                    _carrinhoRepository.RemoverTodos(clienteId);

                    _unitOfWork.SaveChanges();
                    _unitOfWork.Commit();

                    return ResultadoOperacao<PedidoModel>.Ok(pedido, "Pedido realizado com sucesso!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    _unitOfWork.Rollback();
                    // conflito de concorrência: tenta de novo no próximo loop
                    continue;
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();
                    return ResultadoOperacao<PedidoModel>.Erro($"Erro ao processar pedido: {ex.Message}");
                }
            }

            return ResultadoOperacao<PedidoModel>.Erro("Não foi possível processar seu pedido no momento devido a alta demanda. Tente novamente.");
        }
        public ResultadoOperacao CancelarPeloCliente(int clienteId, int pedidoId)
        {
            var pedido = _pedidoRepository.ObterPorIdEcliente(pedidoId, clienteId);

            if (pedido == null)
            {
                return ResultadoOperacao.Erro("Pedido não encontrado.");
            }

            if (pedido.PedidoStatus != PedidoModel.Status.AguardandoSeparacao &&
                pedido.PedidoStatus != PedidoModel.Status.Separado)
            {
                return ResultadoOperacao.Erro("Este pedido não pode mais ser cancelado.");
            }

            if (DateTime.Now > pedido.PedidoData.AddHours(24))
            {
                return ResultadoOperacao.Erro("O prazo de 24 horas para cancelamento já passou.");
            }

            foreach (var item in pedido.Itens)
            {
                if (item.Produto != null)
                {
                    item.Produto.ProdutoQtdEstoque += item.ItemPedidoQtd;
                }

                _movimentacaoRepository.Adicionar(new Models.MovimentacaoEstoqueModel
                {
                    ProdutoID = item.ProdutoID,
                    MovimentacaoQtd = item.ItemPedidoQtd,
                    MovimentacaoTipo = Models.MovimentacaoEstoqueModel.Tipos.CancelamentoOnline,
                    MovimentacaoData = DateTime.Now,
                    ItemPedidoID = item.ItemPedidoID,
                    MovimentacaoDescricao = "Cancelado pelo cliente"
                });
            }

            pedido.PedidoStatus = PedidoModel.Status.CanceladoCliente;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Pedido cancelado com sucesso. O estorno será processado.");
        }
        public ResultadoOperacao CancelarAposEntrega(int pedidoId, string descricao, int usuarioId)
        {
            var pedido = _pedidoRepository.ObterPorId(pedidoId);

            if (pedido == null)
            {
                return ResultadoOperacao.Erro("Pedido não encontrado.");
            }

            if (pedido.PedidoStatus != PedidoModel.Status.Concluido)
            {
                return ResultadoOperacao.Erro("Este pedido não está concluído.");
            }

            if (pedido.PedidoDataConcluido == null || DateTime.Now > pedido.PedidoDataConcluido.Value.AddHours(24))
            {
                return ResultadoOperacao.Erro("O prazo de 24 horas para cancelamento após a entrega já passou.");
            }

            foreach (var item in pedido.Itens)
            {
                if (item.Produto != null)
                {
                    item.Produto.ProdutoQtdEstoque += item.ItemPedidoQtd;
                }

                _movimentacaoRepository.Adicionar(new Models.MovimentacaoEstoqueModel
                {
                    ProdutoID = item.ProdutoID,
                    MovimentacaoQtd = item.ItemPedidoQtd,
                    MovimentacaoTipo = Models.MovimentacaoEstoqueModel.Tipos.CancelamentoOnline,
                    MovimentacaoData = DateTime.Now,
                    ItemPedidoID = item.ItemPedidoID,
                    MovimentacaoDescricao = descricao,
                    UsuarioID = usuarioId
                });
            }

            pedido.PedidoStatus = PedidoModel.Status.CanceladoAdmin;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Pedido cancelado com sucesso. O estorno será processado.");
        }
        public ResultadoOperacao MarcarComoSeparado(int pedidoId, int usuarioId)
        {
            var pedido = _pedidoRepository.ObterPorId(pedidoId);

            if (pedido == null)
            {
                return ResultadoOperacao.Erro("Pedido não encontrado.");
            }

            if (pedido.PedidoStatus != PedidoModel.Status.AguardandoSeparacao)
            {
                return ResultadoOperacao.Erro("Este pedido não está aguardando separação.");
            }

            pedido.PedidoStatus = PedidoModel.Status.Separado;
            pedido.PedidoDataSeparado = DateTime.Now;
            pedido.UsuarioSeparouID = usuarioId;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Pedido marcado como separado!");
        }

        public ResultadoOperacao VoltarParaSeparacao(int pedidoId)
        {
            var pedido = _pedidoRepository.ObterPorId(pedidoId);

            if (pedido == null)
            {
                return ResultadoOperacao.Erro("Pedido não encontrado.");
            }

            if (pedido.PedidoStatus != PedidoModel.Status.Separado)
            {
                return ResultadoOperacao.Erro("Este pedido não está separado.");
            }

            pedido.PedidoStatus = PedidoModel.Status.AguardandoSeparacao;
            pedido.PedidoDataSeparado = null;
            pedido.UsuarioSeparouID = null;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Pedido voltou para aguardando separação.");
        }

        public ResultadoOperacao ConfirmarEntrega(int pedidoId, string codigoInformado, int usuarioId)
        {
            var pedido = _pedidoRepository.ObterPorId(pedidoId);

            if (pedido == null)
            {
                return ResultadoOperacao.Erro("Pedido não encontrado.");
            }

            if (pedido.PedidoStatus != PedidoModel.Status.Separado)
            {
                return ResultadoOperacao.Erro("Este pedido ainda não foi separado.");
            }

            if (pedido.PedidoCodigo != codigoInformado)
            {
                return ResultadoOperacao.Erro("Código incorreto. Confira com o cliente.", campoErro: "codigo");
            }

            pedido.PedidoStatus = PedidoModel.Status.Concluido;
            pedido.PedidoDataConcluido = DateTime.Now;
            pedido.UsuarioEntregouID = usuarioId;

            _unitOfWork.SaveChanges();

            return ResultadoOperacao.Ok("Entrega confirmada com sucesso!");
        }

        public int ExpirarPedidosNaoRetirados()
        {
            var pedidosSeparados = _pedidoRepository.ObterPorStatus(PedidoModel.Status.Separado);
            var limite = DateTime.Now.AddHours(-48);

            var expirados = pedidosSeparados
                .Where(p => p.PedidoDataSeparado.HasValue && p.PedidoDataSeparado.Value <= limite)
                .ToList();

            foreach (var pedido in expirados)
            {
                pedido.PedidoStatus = PedidoModel.Status.CanceladoNaoRetirado;

                foreach (var item in pedido.Itens)
                {
                    if (item.Produto != null)
                    {
                        item.Produto.ProdutoQtdEstoque += item.ItemPedidoQtd;
                    }

                    _movimentacaoRepository.Adicionar(new Models.MovimentacaoEstoqueModel
                    {
                        ProdutoID = item.ProdutoID,
                        MovimentacaoQtd = item.ItemPedidoQtd,
                        MovimentacaoTipo = Models.MovimentacaoEstoqueModel.Tipos.CancelamentoOnline,
                        MovimentacaoData = DateTime.Now,
                        ItemPedidoID = item.ItemPedidoID,
                        MovimentacaoDescricao = "Expirado - nao retirado (48h)"
                    });
                }
            }

            if (expirados.Count > 0)
            {
                _unitOfWork.SaveChanges();
            }

            return expirados.Count;
        }
    }
}