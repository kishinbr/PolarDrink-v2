using NSubstitute;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;

namespace PolarDrinks.Tests.Estoque;

public class EstoqueServiceCadastroTests
{
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository = Substitute.For<IMovimentacaoEstoqueRepository>();
    private readonly IArmazenamentoService _armazenamento = Substitute.For<IArmazenamentoService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EstoqueService _service;

    public EstoqueServiceCadastroTests()
    {
        _armazenamento.SalvarImagemProduto(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns("uploads/produtos/7.jpg");

        _service = new EstoqueService(_produtoRepository, _movimentacaoRepository, _armazenamento, _unitOfWork);
    }

    [Fact]
    public void CadastrarProduto_ComImagem_DeveGravarEConfirmar()
    {
        var produto = CriarProduto();
        using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

        var resultado = _service.CadastrarProduto(produto, new List<int> { 1 }, (conteudo, "foto.jpg"));

        Assert.True(resultado.Sucesso);
        Assert.Equal("uploads/produtos/7.jpg", produto.ProdutoImagemUrl);
        _unitOfWork.Received(1).Commit();
        _unitOfWork.DidNotReceive().Rollback();
        _armazenamento.DidNotReceive().RemoverImagemProduto(Arg.Any<string?>());
    }

    [Fact]
    public void CadastrarProduto_SemImagem_NaoDeveSalvarArquivo()
    {
        var produto = CriarProduto();

        var resultado = _service.CadastrarProduto(produto, new List<int> { 1 }, null);

        Assert.True(resultado.Sucesso);
        Assert.Null(produto.ProdutoImagemUrl);
        _armazenamento.DidNotReceive().SalvarImagemProduto(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<int>());
        _unitOfWork.Received(1).Commit();
    }

    [Fact]
    public void CadastrarProduto_FalhaAoGravarCategorias_DeveDesfazerERemoverAImagem()
    {
        var produto = CriarProduto();
        using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

        // A primeira gravação (produto) funciona; a segunda (categorias) falha
        var gravacoes = 0;
        _unitOfWork.When(u => u.SaveChanges()).Do(_ =>
        {
            gravacoes++;
            if (gravacoes == 2)
            {
                throw new InvalidOperationException("falha ao gravar categorias");
            }
        });

        var resultado = _service.CadastrarProduto(produto, new List<int> { 99999 }, (conteudo, "foto.jpg"));

        Assert.False(resultado.Sucesso);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
        _armazenamento.Received(1).RemoverImagemProduto("uploads/produtos/7.jpg");
    }

    [Fact]
    public void CadastrarProduto_FalhaAoGravarOProduto_NaoDeveSalvarArquivoNenhum()
    {
        var produto = CriarProduto();
        using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

        _unitOfWork.When(u => u.SaveChanges()).Do(_ => throw new InvalidOperationException("falha ao gravar produto"));

        var resultado = _service.CadastrarProduto(produto, new List<int>(), (conteudo, "foto.jpg"));

        Assert.False(resultado.Sucesso);
        _unitOfWork.Received(1).Rollback();
        _unitOfWork.DidNotReceive().Commit();
        _armazenamento.DidNotReceive().SalvarImagemProduto(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public void CadastrarProduto_CodigoDeBarrasDuplicado_DeveRejeitarSemAbrirTransacao()
    {
        var produto = CriarProduto();
        _produtoRepository.ExisteCodigoBarra("7890000000001").Returns(true);

        var resultado = _service.CadastrarProduto(produto, new List<int>(), null);

        Assert.False(resultado.Sucesso);
        _unitOfWork.DidNotReceive().BeginTransaction();
        _produtoRepository.DidNotReceive().Adicionar(Arg.Any<ProdutoModel>());
    }

    private static ProdutoModel CriarProduto()
    {
        return new ProdutoModel
        {
            ProdutoNome = "Vodka Gelada",
            ProdutoCodBarra = "7890000000001",
            ProdutoPrecoVenda = 100m,
            ProdutoPrecoCusto = 40m,
            ProdutoQtdEstoque = 10,
            ProdutoEstoqueMinimo = 2,
            ProdutoAtivo = true
        };
    }
}