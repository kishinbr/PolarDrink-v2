using PolarDrinks.Services.Common;

namespace PolarDrinks.Tests;

public class SmokeTests
{
    [Fact]
    public void ResultadoOperacao_Ok_DeveIndicarSucesso()
    {
        var resultado = ResultadoOperacao.Ok("feito");

        Assert.True(resultado.Sucesso);
        Assert.Equal("feito", resultado.Mensagem);
    }

    [Fact]
    public void ResultadoOperacao_Erro_DeveCarregarMensagemECampo()
    {
        var resultado = ResultadoOperacao.Erro("inválido", "Quantidade");

        Assert.False(resultado.Sucesso);
        Assert.Equal("inválido", resultado.Mensagem);
        Assert.Equal("Quantidade", resultado.CampoErro);
    }
}