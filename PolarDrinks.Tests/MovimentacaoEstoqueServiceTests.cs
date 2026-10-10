using NSubstitute;
using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services;

namespace PolarDrinks.Tests;

public class MovimentacaoEstoqueServiceTests
{
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository = Substitute.For<IMovimentacaoEstoqueRepository>();
    private readonly MovimentacaoEstoqueService _service;

    public MovimentacaoEstoqueServiceTests()
    {
        _service = new MovimentacaoEstoqueService(_produtoRepository, _movimentacaoRepository);
    }

    [Fact]
    public void RegistrarSaida_ComSaldo_DeveReduzirEstoqueEPrepararMovimentacao()
    {
        var produto = CriarProduto(estoque: 10);
        _produtoRepository.ObterPorId(1).Returns(produto);

        var resultado = _service.RegistrarSaida(
            1, 3, MovimentacaoEstoqueModel.Tipos.Saida, usuarioId: 7, itemVendaId: 55);

        Assert.True(resultado.Sucesso);
        Assert.Equal(7, produto.ProdutoQtdEstoque);
        _movimentacaoRepository.Received(1).Adicionar(Arg.Is<MovimentacaoEstoqueModel>(m =>
            m.ProdutoID == 1 &&
            m.MovimentacaoQtd == 3 &&
            m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Saida &&
            m.ItemVendaID == 55 &&
            m.ItemPedidoID == null &&
            m.UsuarioID == 7));
    }

    [Fact]
    public void RegistrarSaida_IgualAoSaldo_DeveZerarOEstoque()
    {
        var produto = CriarProduto(estoque: 3);
        _produtoRepository.ObterPorId(1).Returns(produto);

        var resultado = _service.RegistrarSaida(1, 3, MovimentacaoEstoqueModel.Tipos.Saida, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.Equal(0, produto.ProdutoQtdEstoque);
    }

    [Fact]
    public void RegistrarSaida_SaldoInsuficiente_DeveRejeitarSemEfeitos()
    {
        var produto = CriarProduto(estoque: 2);
        _produtoRepository.ObterPorId(1).Returns(produto);

        var resultado = _service.RegistrarSaida(1, 3, MovimentacaoEstoqueModel.Tipos.Saida, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertSemEfeitos(produto, estoqueEsperado: 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void RegistrarSaida_QuantidadeZeroOuNegativa_DeveRejeitarSemEfeitos(int quantidade)
    {
        var produto = CriarProduto(estoque: 5);
        _produtoRepository.ObterPorId(1).Returns(produto);

        var resultado = _service.RegistrarSaida(1, quantidade, MovimentacaoEstoqueModel.Tipos.Saida, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertSemEfeitos(produto, estoqueEsperado: 5);
    }

    [Fact]
    public void RegistrarSaida_ProdutoInexistente_DeveRejeitar()
    {
        _produtoRepository.ObterPorId(99).Returns((ProdutoModel?)null);

        var resultado = _service.RegistrarSaida(99, 1, MovimentacaoEstoqueModel.Tipos.Saida, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        _movimentacaoRepository.DidNotReceive().Adicionar(Arg.Any<MovimentacaoEstoqueModel>());
    }

    private static ProdutoModel CriarProduto(int estoque)
    {
        return new ProdutoModel
        {
            ProdutoID = 1,
            ProdutoNome = "Produto 1",
            ProdutoQtdEstoque = estoque
        };
    }

    private void AssertSemEfeitos(ProdutoModel produto, int estoqueEsperado)
    {
        Assert.Equal(estoqueEsperado, produto.ProdutoQtdEstoque);
        _movimentacaoRepository.DidNotReceive().Adicionar(Arg.Any<MovimentacaoEstoqueModel>());
    }
}