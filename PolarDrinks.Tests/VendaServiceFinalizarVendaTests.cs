using NSubstitute;
using PolarDrinks.Models;
using PolarDrinks.Models.Requests;
using PolarDrinks.Repositories;
using PolarDrinks.Services;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Tests;

public class VendaServiceFinalizarVendaTests
{
    private readonly IVendaRepository _vendaRepository = Substitute.For<IVendaRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly VendaService _service;

    // A venda que o serviço montou e entregou ao repository (null se nada foi gravado)
    private VendaModel? _vendaGravada;

    public VendaServiceFinalizarVendaTests()
    {
        _vendaRepository
            .When(r => r.Adicionar(Arg.Any<VendaModel>()))
            .Do(chamada => _vendaGravada = chamada.Arg<VendaModel>());

        // Por padrão, o componente de estoque aceita a saída
        _estoque.RegistrarSaida(0, 0, "", null).ReturnsForAnyArgs(ResultadoOperacao.Ok("ok"));

        _service = new VendaService(
            _vendaRepository,
            _produtoRepository,
            _unitOfWork,
            _estoque);
    }

    // ---------- CONTROLE: venda válida ----------

    [Fact]
    public void FinalizarVenda_VendaValida_DeveCalcularPrecoESolicitarSaidaDeEstoque()
    {
        var produto = CriarProduto(id: 1, estoque: 10, preco: 100m, promocao: 15m);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, 2));

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(_vendaGravada);
        Assert.Equal(85m, _vendaGravada!.Itens[0].ItemVendaPreco);
        Assert.Equal(40m, _vendaGravada.Itens[0].ItemVendaCusto);
        Assert.Equal(170m, _vendaGravada.VendaValorTotal);

        // O saldo e a movimentação agora são responsabilidade do componente de estoque
        _estoque.Received(1).RegistrarSaida(1, 2, MovimentacaoEstoqueModel.Tipos.Saida, 7, 0, null);
        _unitOfWork.Received(1).Commit();
    }

    // ---------- FALHA NO MEIO: o estoque recusa a saída depois de a venda ser preparada ----------

    [Fact]
    public void FinalizarVenda_QuandoEstoqueRecusaASaida_DeveDesfazerTudo()
    {
        var produto = CriarProduto(id: 1, estoque: 10);
        ConfigurarProdutos(produto);
        _estoque.RegistrarSaida(0, 0, "", null)
            .ReturnsForAnyArgs(ResultadoOperacao.Erro("Estoque insuficiente"));
        var request = CriarRequest((1, 2));

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
    }

    // ---------- CONTRATO: o servidor define os campos internos ----------

    [Fact]
    public void FinalizarVenda_VendaValida_ServidorDefineDataUsuarioEStatus()
    {
        var produto = CriarProduto(id: 1, estoque: 10);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, 1));

        var antes = DateTime.Now;
        var resultado = _service.FinalizarVenda(request, usuarioId: 7);
        var depois = DateTime.Now;

        Assert.True(resultado.Sucesso);
        Assert.NotNull(_vendaGravada);
        Assert.Equal(7, _vendaGravada!.UsuarioID);
        Assert.False(_vendaGravada.VendaCancelada);
        Assert.Equal(0, _vendaGravada.VendaID);
        Assert.InRange(_vendaGravada.VendaData, antes, depois);
    }

    [Fact]
    public void FinalizarVendaRequest_DeveConterApenasOsCamposPermitidos()
    {
        var camposDoRequest = typeof(FinalizarVendaRequest)
            .GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        var camposDoItem = typeof(ItemVendaRequest)
            .GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "Itens", "VendaTipoPagamento" }, camposDoRequest);
        Assert.Equal(new[] { "ItemVendaQtd", "ProdutoID" }, camposDoItem);
    }

    // ---------- PROBLEMA 1: quantidade zero ou negativa ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void FinalizarVenda_QuantidadeZeroOuNegativa_DeveRejeitarSemEfeitos(int quantidade)
    {
        var produto = CriarProduto(id: 1, estoque: 5);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, quantidade));

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

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
        var request = CriarRequest((1, 3), (1, 3)); // 3 + 3 = 6, mas só há 5

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
    }

    // ---------- PROBLEMA 3: produto inativo ----------

    [Fact]
    public void FinalizarVenda_ProdutoInativo_DeveRejeitarSemEfeitos()
    {
        var produto = CriarProduto(id: 1, estoque: 5, ativo: false);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, 1));

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
    }

    // ---------- PROBLEMA 4: tipo de pagamento ----------

    [Theory]
    [InlineData("Dinheiro")]
    [InlineData("Pix")]
    [InlineData("Cartão")]
    public void FinalizarVenda_TipoPagamentoValido_DeveAceitar(string tipoPagamento)
    {
        var produto = CriarProduto(id: 1, estoque: 10);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, 1));
        request.VendaTipoPagamento = tipoPagamento;

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.True(resultado.Sucesso);
    }

    [Theory]
    [InlineData("Cheque")]
    [InlineData("pix")]
    [InlineData("   ")]
    public void FinalizarVenda_TipoPagamentoInvalido_DeveRejeitarSemEfeitos(string tipoPagamento)
    {
        var produto = CriarProduto(id: 1, estoque: 5);
        ConfigurarProdutos(produto);
        var request = CriarRequest((1, 1));
        request.VendaTipoPagamento = tipoPagamento;

        var resultado = _service.FinalizarVenda(request, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        AssertNenhumEfeitoPersistido(produto, estoqueEsperado: 5);
        _unitOfWork.DidNotReceive().BeginTransaction();
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

    private static FinalizarVendaRequest CriarRequest(params (int produtoId, int quantidade)[] itens)
    {
        var request = new FinalizarVendaRequest
        {
            VendaTipoPagamento = VendaModel.TipoPagamento.Dinheiro
        };

        foreach (var (produtoId, quantidade) in itens)
        {
            request.Itens.Add(new ItemVendaRequest
            {
                ProdutoID = produtoId,
                ItemVendaQtd = quantidade
            });
        }

        return request;
    }

    private void ConfigurarProdutos(params ProdutoModel[] produtos)
    {
        _produtoRepository.ObterPorIds(Arg.Any<List<int>>()).Returns(produtos.ToList());
    }

    private void AssertNenhumEfeitoPersistido(ProdutoModel produto, int estoqueEsperado)
    {
        Assert.Equal(estoqueEsperado, produto.ProdutoQtdEstoque);
        Assert.Null(_vendaGravada);
        _vendaRepository.DidNotReceive().Adicionar(Arg.Any<VendaModel>());
        _estoque.DidNotReceiveWithAnyArgs().RegistrarSaida(0, 0, "", null);
        _unitOfWork.DidNotReceive().Commit();
    }
}