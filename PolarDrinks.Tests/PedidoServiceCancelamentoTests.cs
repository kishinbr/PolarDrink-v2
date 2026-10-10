using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PolarDrinks.Models;
using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;
using PolarDrinks.Repositories.Loja;
using PolarDrinks.Services;
using PolarDrinks.Services.Common;
using PolarDrinks.Services.Loja;

namespace PolarDrinks.Tests;

public class PedidoServiceCancelamentoTests
{
    private const int ClienteId = 5;

    private readonly IPedidoRepository _pedidoRepository = Substitute.For<IPedidoRepository>();
    private readonly ICarrinhoRepository _carrinhoRepository = Substitute.For<ICarrinhoRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly PedidoService _service;

    public PedidoServiceCancelamentoTests()
    {
        // Por padrão, o componente de estoque aceita a devolução
        _estoque.RegistrarDevolucao(0, 0, "", null).ReturnsForAnyArgs(ResultadoOperacao.Ok("ok"));

        _service = new PedidoService(
            _pedidoRepository, _carrinhoRepository, _produtoRepository, _unitOfWork, _estoque);
    }

    // ---------- Cancelamento pelo cliente ----------

    [Fact]
    public void CancelarPeloCliente_PedidoAguardando_DevolveItensEMudaStatus()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.AguardandoSeparacao);
        _pedidoRepository.ObterPorIdEcliente(1, ClienteId).Returns(pedido);

        var resultado = _service.CancelarPeloCliente(ClienteId, 1);

        Assert.True(resultado.Sucesso);
        Assert.Equal(PedidoModel.Status.CanceladoCliente, pedido.PedidoStatus);
        _estoque.Received(1).RegistrarDevolucao(
            1, 2, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, null, "Cancelado pelo cliente", null, 11);
        _estoque.Received(1).RegistrarDevolucao(
            2, 1, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, null, "Cancelado pelo cliente", null, 12);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void CancelarPeloCliente_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.AguardandoSeparacao);
        _pedidoRepository.ObterPorIdEcliente(1, ClienteId).Returns(pedido);

        var primeira = _service.CancelarPeloCliente(ClienteId, 1);
        var segunda = _service.CancelarPeloCliente(ClienteId, 1);

        Assert.True(primeira.Sucesso);
        Assert.False(segunda.Sucesso);

        // O pedido tem 2 itens: só a primeira tentativa devolveu (2 chamadas no total, não 4)
        _estoque.ReceivedWithAnyArgs(2).RegistrarDevolucao(0, 0, "", null);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void CancelarPeloCliente_QuandoEstoqueRecusa_NaoDeveMudarStatusNemGravar()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.AguardandoSeparacao);
        _pedidoRepository.ObterPorIdEcliente(1, ClienteId).Returns(pedido);
        _estoque.RegistrarDevolucao(0, 0, "", null)
            .ReturnsForAnyArgs(ResultadoOperacao.Erro("Este item já foi devolvido ao estoque."));

        var resultado = _service.CancelarPeloCliente(ClienteId, 1);

        Assert.False(resultado.Sucesso);
        Assert.Equal(PedidoModel.Status.AguardandoSeparacao, pedido.PedidoStatus);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().SaveChanges();
    }

    // ---------- Cancelamento após a entrega (admin) ----------

    [Fact]
    public void CancelarAposEntrega_PedidoConcluido_DevolveItensEMudaStatus()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.Concluido, concluidoEm: DateTime.Now.AddHours(-1));
        _pedidoRepository.ObterPorId(1).Returns(pedido);

        var resultado = _service.CancelarAposEntrega(1, "Produto com defeito", usuarioId: 9);

        Assert.True(resultado.Sucesso);
        Assert.Equal(PedidoModel.Status.CanceladoAdmin, pedido.PedidoStatus);
        _estoque.Received(1).RegistrarDevolucao(
            1, 2, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, 9, "Produto com defeito", null, 11);
        _estoque.Received(1).RegistrarDevolucao(
            2, 1, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, 9, "Produto com defeito", null, 12);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void CancelarAposEntrega_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.Concluido, concluidoEm: DateTime.Now.AddHours(-1));
        _pedidoRepository.ObterPorId(1).Returns(pedido);

        var primeira = _service.CancelarAposEntrega(1, "Produto com defeito", usuarioId: 9);
        var segunda = _service.CancelarAposEntrega(1, "Produto com defeito", usuarioId: 9);

        Assert.True(primeira.Sucesso);
        Assert.False(segunda.Sucesso);
        _estoque.ReceivedWithAnyArgs(2).RegistrarDevolucao(0, 0, "", null);
        _unitOfWork.Received(1).SaveChanges();
    }

    // ---------- Expiração ----------

    [Fact]
    public void ExpirarPedidos_DevolveSomenteOsExpirados()
    {
        var expirado = CriarPedido(1, PedidoModel.Status.Separado, separadoEm: DateTime.Now.AddHours(-30));
        var recente = CriarPedido(2, PedidoModel.Status.Separado, separadoEm: DateTime.Now.AddHours(-2));
        _pedidoRepository.ObterPorStatus(PedidoModel.Status.Separado)
            .Returns(new List<PedidoModel> { expirado, recente });
        _pedidoRepository.ObterPorId(1).Returns(expirado);

        var quantidade = _service.ExpirarPedidosNaoRetirados();

        Assert.Equal(1, quantidade);
        Assert.Equal(PedidoModel.Status.CanceladoNaoRetirado, expirado.PedidoStatus);
        Assert.Equal(PedidoModel.Status.Separado, recente.PedidoStatus);
        _estoque.Received(1).RegistrarDevolucao(
            1, 2, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, null, "Expirado", null, 11);
        _estoque.Received(1).RegistrarDevolucao(
            2, 1, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, null, "Expirado", null, 12);
        _estoque.ReceivedWithAnyArgs(2).RegistrarDevolucao(0, 0, "", null);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void ExpirarPedidos_PedidoComDevolucaoRecusada_NaoAfetaOsDemais()
    {
        var pedido1 = CriarPedido(1, PedidoModel.Status.Separado, separadoEm: DateTime.Now.AddHours(-30));
        var pedido2 = CriarPedido(2, PedidoModel.Status.Separado, separadoEm: DateTime.Now.AddHours(-30));
        _pedidoRepository.ObterPorStatus(PedidoModel.Status.Separado)
            .Returns(new List<PedidoModel> { pedido1, pedido2 });
        _pedidoRepository.ObterPorId(1).Returns(pedido1);
        _pedidoRepository.ObterPorId(2).Returns(pedido2);

        // O primeiro item do pedido 1 já tinha sido devolvido: o estoque recusa
        _estoque.RegistrarDevolucao(
                1, 2, MovimentacaoEstoqueModel.Tipos.CancelamentoOnline, null, "Expirado", null, 11)
            .Returns(ResultadoOperacao.Erro("Este item já foi devolvido ao estoque."));

        var quantidade = _service.ExpirarPedidosNaoRetirados();

        Assert.Equal(1, quantidade);
        Assert.Equal(PedidoModel.Status.CanceladoNaoRetirado, pedido2.PedidoStatus);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void ExpirarPedidos_ConflitoDeConcorrenciaNaGravacao_NaoDeveContarNemQuebrar()
    {
        var pedido = CriarPedido(1, PedidoModel.Status.Separado, separadoEm: DateTime.Now.AddHours(-30));
        _pedidoRepository.ObterPorStatus(PedidoModel.Status.Separado)
            .Returns(new List<PedidoModel> { pedido });
        _pedidoRepository.ObterPorId(1).Returns(pedido);
        _unitOfWork
            .When(u => u.SaveChanges())
            .Do(_ => throw new DbUpdateConcurrencyException("conflito"));

        var quantidade = _service.ExpirarPedidosNaoRetirados();

        Assert.Equal(0, quantidade);
        _unitOfWork.Received(1).Rollback();
    }

    // ---------- Método auxiliar ----------

    private static PedidoModel CriarPedido(
        int id, string status, DateTime? separadoEm = null, DateTime? concluidoEm = null)
    {
        var pedido = new PedidoModel
        {
            PedidoID = id,
            PedidoCodigo = "A1B2",
            ClienteID = ClienteId,
            PedidoData = DateTime.Now,
            PedidoStatus = status,
            PedidoDataSeparado = separadoEm,
            PedidoDataConcluido = concluidoEm
        };

        pedido.Itens.Add(new ItemPedidoModel { ItemPedidoID = id * 10 + 1, ProdutoID = 1, ItemPedidoQtd = 2 });
        pedido.Itens.Add(new ItemPedidoModel { ItemPedidoID = id * 10 + 2, ProdutoID = 2, ItemPedidoQtd = 1 });

        return pedido;
    }
}