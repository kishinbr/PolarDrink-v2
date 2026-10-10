using PolarDrinks.Models;

namespace PolarDrinks.Tests;

public class PrecoFinalTests
{
    [Fact]
    public void CemReaisCom15PorCento_DeveSerOitentaECinco()
    {
        var produto = new ProdutoModel { ProdutoPrecoVenda = 100m, ProdutoPromocao = 15m };

        Assert.Equal(85.00m, produto.CalcularPrecoFinal());
    }

    [Fact]
    public void DeveArredondarParaDuasCasas()
    {
        // 10,05 - 15% = 8,5425
        var produto = new ProdutoModel { ProdutoPrecoVenda = 10.05m, ProdutoPromocao = 15m };

        Assert.Equal(8.54m, produto.CalcularPrecoFinal());
    }

    [Fact]
    public void ArredondamentoComercial_MeioCentavoSobe()
    {
        // 0,25 - 10% = 0,225 -> 0,23 (e não 0,22, que seria o arredondamento "bancário")
        var produto = new ProdutoModel { ProdutoPrecoVenda = 0.25m, ProdutoPromocao = 10m };

        Assert.Equal(0.23m, produto.CalcularPrecoFinal());
    }

    [Fact]
    public void SemPromocao_DeveRetornarPrecoCheio()
    {
        var produto = new ProdutoModel { ProdutoPrecoVenda = 49.90m, ProdutoPromocao = 0m };

        Assert.Equal(49.90m, produto.CalcularPrecoFinal());
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(150)]
    public void PromocaoInvalida_DeveSerIgnorada(int promocao)
    {
        var produto = new ProdutoModel { ProdutoPrecoVenda = 100m, ProdutoPromocao = promocao };

        Assert.Equal(100m, produto.CalcularPrecoFinal());
    }

    [Fact]
    public void PrecoVazio_DeveValerZero()
    {
        var produto = new ProdutoModel { ProdutoPrecoVenda = null, ProdutoPromocao = 10m };

        Assert.Equal(0m, produto.CalcularPrecoFinal());
    }

    [Fact]
    public void TotalDeSeteUnidades_DeveFecharComOPrecoUnitarioArredondado()
    {
        // Antes: total 59,80 (8,5425 x 7 arredondado pelo banco) contra itens 8,54 x 7 = 59,78
        var produto = new ProdutoModel { ProdutoPrecoVenda = 10.05m, ProdutoPromocao = 15m };

        var precoUnitario = produto.CalcularPrecoFinal();

        Assert.Equal(59.78m, precoUnitario * 7);
    }
}