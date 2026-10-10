using NSubstitute;
using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Tests;

public class VendaServiceCancelamentoTests
{
    private readonly IVendaRepository _vendaRepository = Substitute.For<IVendaRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly VendaService _service;

    public VendaServiceCancelamentoTests()
    {
        // Por padrão, o componente de estoque aceita a devolução
        _estoque.RegistrarDevolucao(0, 0, "", null).ReturnsForAnyArgs(ResultadoOperacao.Ok("ok"));

        _service = new VendaService(_vendaRepository, _produtoRepository, _unitOfWork, _estoque);
    }

    [Fact]
    public void CancelarVenda_VendaRecente_DevolveCadaItemEMarcaComoCancelada()
    {
        var venda = CriarVenda();
        _vendaRepository.ObterPorId(10).Returns(venda);

        var resultado = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.True(venda.VendaCancelada);
        _estoque.Received(1).RegistrarDevolucao(
            1, 2, MovimentacaoEstoqueModel.Tipos.Cancelamento, 7, "Cliente desistiu", 55, null);
        _estoque.Received(1).RegistrarDevolucao(
            2, 1, MovimentacaoEstoqueModel.Tipos.Cancelamento, 7, "Cliente desistiu", 56, null);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void CancelarVenda_Repetido_DeveRejeitarSegundaTentativaSemNovaDevolucao()
    {
        var venda = CriarVenda();
        _vendaRepository.ObterPorId(10).Returns(venda);

        var primeira = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);
        var segunda = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);

        Assert.True(primeira.Sucesso);
        Assert.False(segunda.Sucesso);

        // A venda tem 2 itens: só a primeira tentativa devolveu (2 chamadas no total, não 4)
        _estoque.ReceivedWithAnyArgs(2).RegistrarDevolucao(0, 0, "", null);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void CancelarVenda_QuandoEstoqueRecusaADevolucao_NaoDeveCancelarNemGravar()
    {
        var venda = CriarVenda();
        _vendaRepository.ObterPorId(10).Returns(venda);
        _estoque.RegistrarDevolucao(0, 0, "", null)
            .ReturnsForAnyArgs(ResultadoOperacao.Erro("Este item já foi devolvido ao estoque."));

        var resultado = _service.CancelarVenda(10, "Cliente desistiu", usuarioId: 7);

        Assert.False(resultado.Sucesso);
        Assert.False(venda.VendaCancelada);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().SaveChanges();
    }

    [Fact]
    public void CancelarVenda_Apos24Horas_DeveRejeitarSemDevolver()
    {
        var venda = CriarVenda(data: DateTime.Now.AddHours(-25));
        _vendaRepository.ObterPorId(10).Returns(venda);

        var resultado = _service.CancelarVenda(10, "Tarde demais", usuarioId: 7);

        Assert.False(resultado.Sucesso);
        Assert.False(venda.VendaCancelada);
        _estoque.DidNotReceiveWithAnyArgs().RegistrarDevolucao(0, 0, "", null);
        _unitOfWork.DidNotReceive().SaveChanges();
    }

    private static VendaModel CriarVenda(DateTime? data = null)
    {
        var venda = new VendaModel
        {
            VendaID = 10,
            VendaTipoPagamento = "Dinheiro",
            VendaData = data ?? DateTime.Now,
            VendaCancelada = false
        };

        venda.Itens.Add(new ItemVendaModel { ItemVendaID = 55, ProdutoID = 1, ItemVendaQtd = 2 });
        venda.Itens.Add(new ItemVendaModel { ItemVendaID = 56, ProdutoID = 2, ItemVendaQtd = 1 });

        return venda;
    }
}