using NSubstitute;
using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Modules.Compras.Models;
using PolarDrinks.Modules.Compras.Repositories;
using PolarDrinks.Modules.Compras.Services;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;

namespace PolarDrinks.Tests.Compras;

public class CompraEstoqueServiceConfirmarEntregaTests
{
    private readonly ICompraEstoqueRepository _compraRepository = Substitute.For<ICompraEstoqueRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IFornecedorRepository _fornecedorRepository = Substitute.For<IFornecedorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly CompraEstoqueService _service;

    public CompraEstoqueServiceConfirmarEntregaTests()
    {
        // Por padrão, o componente de estoque aceita a entrada
        _estoque.RegistrarEntrada(0, 0, null, 0).ReturnsForAnyArgs(ResultadoOperacao.Ok("ok"));

        _service = new CompraEstoqueService(
            _compraRepository, _produtoRepository, _fornecedorRepository, _unitOfWork, _estoque);
    }

    [Fact]
    public void ConfirmarEntrega_CompraAguardando_SolicitaEntradaDeCadaItemEConclui()
    {
        var compra = CriarCompra();
        _compraRepository.ObterParaConfirmarEntrega(10).Returns(compra);

        var resultado = _service.ConfirmarEntrega(10, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Concluído", compra.CompraStatus);
        Assert.NotNull(compra.CompraDataEntrega);
        _estoque.Received(1).RegistrarEntrada(1, 5, 7, 31);
        _estoque.Received(1).RegistrarEntrada(2, 3, 7, 32);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void ConfirmarEntrega_Repetido_NaoDeveGerarNovaEntrada()
    {
        var compra = CriarCompra();
        _compraRepository.ObterParaConfirmarEntrega(10).Returns(compra);

        var primeira = _service.ConfirmarEntrega(10, usuarioId: 7);
        var segunda = _service.ConfirmarEntrega(10, usuarioId: 7);

        Assert.True(primeira.Sucesso);
        Assert.True(segunda.Sucesso);

        // A compra tem 2 itens: só a primeira confirmação deu entrada (2 chamadas no total, não 4)
        _estoque.ReceivedWithAnyArgs(2).RegistrarEntrada(0, 0, null, 0);
        _unitOfWork.Received(1).SaveChanges();
    }

    [Fact]
    public void ConfirmarEntrega_QuandoEstoqueRecusaAEntrada_NaoDeveConcluirNemGravar()
    {
        var compra = CriarCompra();
        _compraRepository.ObterParaConfirmarEntrega(10).Returns(compra);
        _estoque.RegistrarEntrada(0, 0, null, 0)
            .ReturnsForAnyArgs(ResultadoOperacao.Erro("Produto não encontrado: ID 1"));

        var resultado = _service.ConfirmarEntrega(10, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Aguardando", compra.CompraStatus);
        Assert.Null(compra.CompraDataEntrega);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().SaveChanges();
    }

    [Fact]
    public void ConfirmarEntrega_CompraInexistente_DeveRejeitarSemEntrada()
    {
        _compraRepository.ObterParaConfirmarEntrega(99).Returns((CompraEstoqueModel?)null);

        var resultado = _service.ConfirmarEntrega(99, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        _estoque.DidNotReceiveWithAnyArgs().RegistrarEntrada(0, 0, null, 0);
        _unitOfWork.DidNotReceive().SaveChanges();
    }

    private static CompraEstoqueModel CriarCompra()
    {
        var compra = new CompraEstoqueModel { CompraID = 10, CompraStatus = "Aguardando" };

        compra.Itens.Add(new ItemCompraModel { ItemCompraID = 31, ProdutoID = 1, ItemCompraQtd = 5 });
        compra.Itens.Add(new ItemCompraModel { ItemCompraID = 32, ProdutoID = 2, ItemCompraQtd = 3 });

        return compra;
    }
}