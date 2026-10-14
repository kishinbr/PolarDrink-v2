using PolarDrinks.Modules.Dashboard.Services;

namespace PolarDrinks.Tests.Dashboard;

public class PrevisaoLinearTests
{
    // ---------- Previsão do próximo ponto ----------

    [Fact]
    public void PreverProximo_SerieCrescente_DeveContinuarATendencia()
    {
        // y = 10x: o próximo ponto (x = 4) é 40
        Assert.Equal(40m, PrevisaoLinear.PreverProximo(new[] { 10m, 20m, 30m }));
    }

    [Fact]
    public void PreverProximo_SerieDecrescente_DeveContinuarATendencia()
    {
        // y = 60 - 10x: o próximo ponto (x = 4) é 20
        Assert.Equal(20m, PrevisaoLinear.PreverProximo(new[] { 50m, 40m, 30m }));
    }

    [Fact]
    public void PreverProximo_SerieConstante_DeveRepetirOValor()
    {
        Assert.Equal(100m, PrevisaoLinear.PreverProximo(new[] { 100m, 100m, 100m }));
    }

    [Fact]
    public void PreverProximo_DoisPontos_DeveUsarARetaEntreEles()
    {
        Assert.Equal(300m, PrevisaoLinear.PreverProximo(new[] { 100m, 200m }));
    }

    [Fact]
    public void PreverProximo_SerieComSalto_DeveCalcularPeloMinimosQuadrados()
    {
        // a = 100, b = -33,33...: o próximo ponto (x = 4) é 366,67
        var previsao = PrevisaoLinear.PreverProximo(new[] { 100m, 100m, 300m });

        Assert.Equal(366.67m, Math.Round(previsao, 2));
    }

    [Fact]
    public void PreverProximo_PrevisaoNegativa_DeveSerTruncadaEmZero()
    {
        // a = -8, b = 18: o próximo ponto (x = 3) seria -6
        Assert.Equal(0m, PrevisaoLinear.PreverProximo(new[] { 10m, 2m }));
    }

    [Fact]
    public void PreverProximo_SerieVazia_DeveSerZero()
    {
        Assert.Equal(0m, PrevisaoLinear.PreverProximo(Array.Empty<decimal>()));
    }

    [Fact]
    public void PreverProximo_UmUnicoPonto_DeveSerZero()
    {
        Assert.Equal(0m, PrevisaoLinear.PreverProximo(new[] { 100m }));
    }

    // ---------- Inclinação da tendência ----------

    [Fact]
    public void Inclinacao_SerieCrescente_DeveSerPositiva()
    {
        Assert.Equal(2.0, PrevisaoLinear.Inclinacao(new[] { 2, 4, 6 }));
    }

    [Fact]
    public void Inclinacao_SerieDecrescente_DeveSerNegativa()
    {
        Assert.Equal(-2.0, PrevisaoLinear.Inclinacao(new[] { 6, 4, 2 }));
    }

    [Fact]
    public void Inclinacao_SerieConstante_DeveSerZero()
    {
        Assert.Equal(0.0, PrevisaoLinear.Inclinacao(new[] { 5, 5, 5 }));
    }

    [Fact]
    public void Inclinacao_SerieVazia_DeveSerNula()
    {
        Assert.Null(PrevisaoLinear.Inclinacao(Array.Empty<int>()));
    }

    [Fact]
    public void Inclinacao_UmUnicoPonto_DeveSerNula()
    {
        Assert.Null(PrevisaoLinear.Inclinacao(new[] { 7 }));
    }
}