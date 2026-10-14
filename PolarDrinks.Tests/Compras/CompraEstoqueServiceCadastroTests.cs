using NSubstitute;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Modules.Compras.Models;
using PolarDrinks.Modules.Compras.Repositories;
using PolarDrinks.Modules.Compras.Services;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;

namespace PolarDrinks.Tests.Compras;

public class CompraEstoqueServiceCadastroTests
{
    private readonly ICompraEstoqueRepository _compraRepository = Substitute.For<ICompraEstoqueRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IFornecedorRepository _fornecedorRepository = Substitute.For<IFornecedorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMovimentacaoEstoqueService _estoque = Substitute.For<IMovimentacaoEstoqueService>();
    private readonly CompraEstoqueService _service;

    // A compra que o serviço montou e entregou ao repository (null se nada foi gravado)
    private CompraEstoqueModel? _compraGravada;

    public CompraEstoqueServiceCadastroTests()
    {
        _compraRepository
            .When(r => r.Adicionar(Arg.Any<CompraEstoqueModel>()))
            .Do(chamada => _compraGravada = chamada.Arg<CompraEstoqueModel>());

        _service = new CompraEstoqueService(
            _compraRepository, _produtoRepository, _fornecedorRepository, _unitOfWork, _estoque);
    }

    [Fact]
    public void CadastrarCompra_ItensValidos_DeveGravarCompraAguardando()
    {
        var itens = new List<(int? ProdutoId, int Quantidade, decimal Preco)>
        {
            (1, 5, 12.50m),
            (2, 3, 8m)
        };

        var resultado = _service.CadastrarCompra(3, itens, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(_compraGravada);
        Assert.Equal(3, _compraGravada!.FornecedorID);
        Assert.Equal("Aguardando", _compraGravada.CompraStatus);
        Assert.Equal(7, _compraGravada.UsuarioID);
        Assert.Equal(2, _compraGravada.Itens.Count);
        _compraRepository.Received(1).SalvarAlteracoes();
    }

    [Fact]
    public void CadastrarCompra_DeveIgnorarItensInvalidos()
    {
        var itens = new List<(int? ProdutoId, int Quantidade, decimal Preco)>
        {
            (1, 5, 12.50m),     // válido
            (null, 5, 12.50m),  // sem produto
            (2, 0, 8m),         // quantidade zero
            (3, 2, 0m)          // preço zero
        };

        var resultado = _service.CadastrarCompra(3, itens, usuarioId: 7);

        Assert.True(resultado.Sucesso);
        var item = Assert.Single(_compraGravada!.Itens);
        Assert.Equal(1, item.ProdutoID);
        Assert.Equal(5, item.ItemCompraQtd);
        Assert.Equal(12.50m, item.ItemCompraPreco);
    }

    [Fact]
    public void CadastrarCompra_SemItensValidos_DeveRejeitarSemGravar()
    {
        var itens = new List<(int? ProdutoId, int Quantidade, decimal Preco)>
        {
            (null, 5, 12.50m),
            (2, 0, 8m)
        };

        var resultado = _service.CadastrarCompra(3, itens, usuarioId: 7);

        Assert.False(resultado.Sucesso);
        Assert.Null(_compraGravada);
        _compraRepository.DidNotReceive().SalvarAlteracoes();
    }
}