using NSubstitute;
using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services;

namespace PolarDrinks.Tests;

public class VendaServiceFinalizarVendaTests
{
    private readonly IVendaRepository _vendaRepository = Substitute.For<IVendaRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository = Substitute.For<IMovimentacaoEstoqueRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly VendaService _service;

    public VendaServiceFinalizarVendaTests()
    {
        _service = new VendaService(
            _vendaRepository,
            _produtoRepository,
            _movimentacaoRepository,
            _unitOfWork);
    }

    // ---------- CONTROLE: venda válida (deve passar antes e depois da correção) ----------

    [Fact]
    public void FinalizarVenda_VendaValida_DeveCalcularPrecoReduzirEstoqueERegistrarMovimentacao()
    {
        var produto = CriarProduto(id: 1, estoque: 10, preco: 100m, promocao: 15m);
        ConfigurarProdutos(produto);
        var venda = CriarVenda((1, 2));

        var resultado = _service.FinalizarVenda(venda, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.Equal(85m, venda.Itens[0].ItemVendaPreco);
        Assert.Equal(170m, venda.VendaValorTotal);
        Assert.Equal(8, produto.ProdutoQtdEstoque);

        _vendaRepository.Received(1).Adicionar(venda);
        _movimentacaoRepository.Received(1).Adicionar(Arg.Is<MovimentacaoEstoqueModel>(m =>
            m.ProdutoID == 1 &&
            m.MovimentacaoQtd == 2 &&
            m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Saida));
        _unitOfWork.Received(1).Commit();
    }

    // ---------- PROBLEMA 1: quantidade zero ou negativa ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void FinalizarVenda_QuantidadeZeroOuNegativa_DeveRejeitarSemEfeitos(int quantidade)
    {
        var produto = CriarProduto(id: 1, estoque: 5);
        ConfigurarProdutos(produto);
        var venda = CriarVenda((1, quantidade));

        var resultado = _service.FinalizarVenda(venda, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    // ---------- PROBLEMA 2: mesmo produto em linhas repetidas ----------

    [Fact]
    public void FinalizarVenda_MesmoProdutoEmDuasLinhas_DeveSomarQuantidadesEVerificarSaldo()
    {
        var produto = CriarProduto(id: 1, estoque: 5);
        ConfigurarProdutos(produto);
        var venda = CriarVenda((1, 3), (1, 3)); // 3 + 3 = 6, mas só há 5

        var resultado = _service.FinalizarVenda(venda, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
    }

    // ---------- PROBLEMA 3: produto inativo ----------

    [Fact]
    public void FinalizarVenda_ProdutoInativo_DeveRejeitarSemEfeitos()
    {
        var produto = CriarProduto(id: 1, estoque: 5, ativo: false);
        ConfigurarProdutos(produto);
        var venda = CriarVenda((1, 1));

        var resultado = _service.FinalizarVenda(venda, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
    }

    // ---------- Métodos auxiliares ----------

    private static ProdutoModel CriarProduto(
        int id, int estoque, bool ativo = true, decimal preco = 100m, decimal promocao = 0m)
    {
        return new ProdutoModel
        {
            ProdutoID = id,
            ProdutoNome = $"Produto {id}",
            ProdutoPrecoVenda = preco,
            ProdutoPrecoCusto = 40m,
            ProdutoPromocao = promocao,
            ProdutoQtdEstoque = estoque,
            ProdutoAtivo = ativo
        };
    }

    private static VendaModel CriarVenda(params (int produtoId, int quantidade)[] itens)
    {
        var venda = new VendaModel { VendaTipoPagamento = "Dinheiro" };
        foreach (var (produtoId, quantidade) in itens)
        {
            venda.Itens.Add(new ItemVendaModel
            {
                ProdutoID = produtoId,
                ItemVendaQtd = quantidade
            });
        }
        return venda;
    }

    private void ConfigurarProdutos(params ProdutoModel[] produtos)
    {
        _produtoRepository.ObterPorIds(Arg.Any<List<int>>()).Returns(produtos.ToList());
    }

    private void AssertNenhumEfeitoPersistido(ProdutoModel produto, int estoqueEsperado)
    {
        Assert.Equal(estoqueEsperado, produto.ProdutoQtdEstoque);
        _vendaRepository.DidNotReceive().Adicionar(Arg.Any<VendaModel>());
        _movimentacaoRepository.DidNotReceive().Adicionar(Arg.Any<MovimentacaoEstoqueModel>());
        _unitOfWork.DidNotReceive().Commit();
    }
}