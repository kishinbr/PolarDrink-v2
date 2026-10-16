namespace PolarDrinks.Tests.Arquitetura;

/// <summary>
/// Regras de arquitetura aplicadas ao código REAL do projeto.
/// Se alguém quebrar um limite entre módulos, o teste correspondente fica vermelho
/// e a mensagem lista cada arquivo e o motivo.
/// </summary>
[Trait("Categoria", "Arquitetura")]
public class ArquiteturaTests
{
    private static readonly Lazy<List<ArquivoFonte>> Fonte = new(CodigoFonte.Carregar);

    private static List<ArquivoFonte> Arquivos => Fonte.Value;

    // Garante que o teste está lendo o projeto de verdade (sem isso, as regras passariam "por não ter o que verificar")
    [Fact]
    public void ProjetoFoiLido_DeveEncontrarOsArquivosDeCadaModulo()
    {
        foreach (var modulo in RegrasDeArquitetura.Modulos)
        {
            Assert.Contains(Arquivos, a => a.Caminho.StartsWith($"Modules/{modulo}/Controllers/"));
            Assert.Contains(Arquivos, a => a.Caminho.StartsWith($"Modules/{modulo}/Services/"));
            Assert.Contains(Arquivos, a => a.Caminho.StartsWith($"Modules/{modulo}/Models/"));
        }

        Assert.Contains(Arquivos, a => a.Caminho.StartsWith("Compartilhado/"));
    }

    [Fact]
    public void PastasDeModulos_DevemSerExatamenteOsDeclaradosNasRegras()
    {
        var existentes = Arquivos
            .Select(a => RegrasDeArquitetura.ModuloDe(a.Caminho))
            .Where(m => m != null && m != "Compartilhado")
            .Select(m => m!)
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        Assert.Equal(RegrasDeArquitetura.Modulos.OrderBy(m => m).ToList(), existentes);
    }

    [Fact]
    public void R1_ModulosSoDependemUnsDosOutrosNasDirecoesPermitidas()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarDependenciasEntreModulos(Arquivos));
    }

    [Fact]
    public void R2_SoOEstoqueUsaORepositoryDeMovimentacoes()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarMovimentacoesSoNoEstoque(Arquivos));
    }

    [Fact]
    public void R3_SoOEstoqueAlteraOSaldoDosProdutos()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(Arquivos));
    }

    [Fact]
    public void R4_ControllersNaoAcessamRepositoriosNemOBanco()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarControllersSemAcessoADados(Arquivos));
    }

    [Fact]
    public void R5_ServicesERepositoriesNaoUsamTiposDaCamadaWeb()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarServicosSemCamadaWeb(Arquivos));
    }

    [Fact]
    public void R6_ModelsNaoDependemDasCamadasDeCima()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarModelsIndependentes(Arquivos));
    }

    [Fact]
    public void R7_CompartilhadoNaoDependeDeModulos()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarCompartilhadoSemModulos(Arquivos));
    }

    [Fact]
    public void R8_NamespaceDeCadaArquivoIgualAoCaminhoDaPasta()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarNamespaceIgualAPasta(Arquivos));
    }

    [Fact]
    public void R9_PastasDeViewsFicamNoMesmoModuloDoController()
    {
        Assert.Empty(RegrasDeArquitetura.VerificarViewsNoModuloDoController(Arquivos));
    }
}
