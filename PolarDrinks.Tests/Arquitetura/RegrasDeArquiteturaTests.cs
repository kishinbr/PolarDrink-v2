namespace PolarDrinks.Tests.Arquitetura;

/// <summary>
/// Prova que cada regra DETECTA a violação que promete detectar.
/// Alimenta o verificador com código propositalmente errado: se uma regra deixasse de funcionar,
/// os testes de ArquiteturaTests passariam "no vazio" e ninguém perceberia.
/// </summary>
[Trait("Categoria", "Arquitetura")]
public class RegrasDeArquiteturaTests
{
    private static ArquivoFonte Cs(string caminho, string texto) => new(caminho, texto);

    // ---------- R1: dependências entre módulos ----------

    [Fact]
    public void R1_DependenciaNaoPermitida_ViaUsing_DeveSerDetectada()
    {
        var arquivo = Cs("Modules/Vendas/Services/X.cs",
            "using PolarDrinks.Modules.Loja.Models;\nnamespace PolarDrinks.Modules.Vendas.Services;\npublic class X { }");

        var violacoes = RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { arquivo });

        Assert.Single(violacoes);
    }

    [Fact]
    public void R1_DependenciaNaoPermitida_ViaNomeQualificado_DeveSerDetectada()
    {
        var arquivo = Cs("Modules/Vendas/Services/X.cs",
            "namespace PolarDrinks.Modules.Vendas.Services;\npublic class X { PolarDrinks.Modules.Compras.Models.CompraEstoqueModel c; }");

        var violacoes = RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { arquivo });

        Assert.Single(violacoes);
    }

    [Fact]
    public void R1_DependenciaPermitida_NaoDeveGerarViolacao()
    {
        var arquivo = Cs("Modules/Vendas/Services/X.cs",
            "using PolarDrinks.Modules.Estoque.Models;\nnamespace PolarDrinks.Modules.Vendas.Services;\npublic class X { }");

        Assert.Empty(RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { arquivo }));
    }

    [Fact]
    public void R1_ExcecaoDeRastreabilidade_ValeSoParaAMovimentacaoDeEstoque()
    {
        const string codigo = "using PolarDrinks.Modules.Vendas.Models;\nnamespace X;\npublic class Y { }";

        var daMovimentacao = Cs("Modules/Estoque/Models/MovimentacaoEstoqueModel.cs", codigo);
        var deOutroArquivo = Cs("Modules/Estoque/Services/EstoqueService.cs", codigo);

        Assert.Empty(RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { daMovimentacao }));
        Assert.Single(RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { deOutroArquivo }));
    }

    [Fact]
    public void R1_ModuloNaoDeclaradoNasRegras_DeveSerDetectado()
    {
        var arquivo = Cs("Modules/Relatorios/Services/X.cs", "namespace PolarDrinks.Modules.Relatorios.Services;");

        Assert.Single(RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { arquivo }));
    }

    [Fact]
    public void ComentariosETextosEntreAspas_DevemSerIgnorados()
    {
        var arquivo = Cs("Modules/Vendas/Services/X.cs",
            "// using PolarDrinks.Modules.Loja.Models;\n" +
            "/* IMovimentacaoEstoqueRepository */\n" +
            "namespace PolarDrinks.Modules.Vendas.Services;\n" +
            "public class X { string a = \"PolarDrinks.Modules.Loja IMovimentacaoEstoqueRepository\"; }");

        Assert.Empty(RegrasDeArquitetura.VerificarDependenciasEntreModulos(new[] { arquivo }));
        Assert.Empty(RegrasDeArquitetura.VerificarMovimentacoesSoNoEstoque(new[] { arquivo }));
    }

    // ---------- R2: movimentações ----------

    [Fact]
    public void R2_RepositoryDeMovimentacoesForaDoEstoque_DeveSerDetectado()
    {
        var fora = Cs("Modules/Vendas/Services/VendaService.cs", "class V { IMovimentacaoEstoqueRepository _m; }");
        var dentro = Cs("Modules/Estoque/Services/EstoqueService.cs", "class E { IMovimentacaoEstoqueRepository _m; }");

        Assert.Single(RegrasDeArquitetura.VerificarMovimentacoesSoNoEstoque(new[] { fora }));
        Assert.Empty(RegrasDeArquitetura.VerificarMovimentacoesSoNoEstoque(new[] { dentro }));
    }

    // ---------- R3: saldo ----------

    [Fact]
    public void R3_AlterarOSaldoForaDoEstoque_DeveSerDetectado()
    {
        var subtraindo = Cs("Modules/Vendas/Services/V.cs", "class V { void M(P p) { p.ProdutoQtdEstoque -= 1; } }");
        var somando = Cs("Modules/Loja/Services/L.cs", "class L { void M(P p) { p.ProdutoQtdEstoque += 1; } }");
        var atribuindo = Cs("Modules/Compras/Services/C.cs", "class C { void M(P p) { p.ProdutoQtdEstoque = 5; } }");

        Assert.Single(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(new[] { subtraindo }));
        Assert.Single(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(new[] { somando }));
        Assert.Single(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(new[] { atribuindo }));
    }

    [Fact]
    public void R3_LerOuCompararOSaldo_NaoDeveSerDetectado()
    {
        var arquivo = Cs("Modules/Vendas/Services/V.cs",
            "class V { bool M(P p) => p.ProdutoQtdEstoque == 0 || p.ProdutoQtdEstoque <= 3 || p.ProdutoQtdEstoque >= 9 || p.ProdutoQtdEstoque != 2; }");

        Assert.Empty(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(new[] { arquivo }));
    }

    [Fact]
    public void R3_AlterarOSaldoDentroDoEstoque_EPermitido()
    {
        var arquivo = Cs("Modules/Estoque/Services/MovimentacaoEstoqueService.cs",
            "class M { void X(P p) { p.ProdutoQtdEstoque -= 1; } }");

        Assert.Empty(RegrasDeArquitetura.VerificarSaldoSoAlteradoPeloEstoque(new[] { arquivo }));
    }

    // ---------- R4: controllers ----------

    [Fact]
    public void R4_ControllerComRepositoryOuBanco_DeveSerDetectado()
    {
        var comRepository = Cs("Modules/Vendas/Controllers/VendaController.cs", "class C { IVendaRepository _r; }");
        var comBanco = Cs("Modules/Vendas/Controllers/OutroController.cs", "using PolarDrinks.Data;\nclass C { }");
        var correto = Cs("Modules/Vendas/Controllers/CertoController.cs", "class C { IVendaService _s; }");

        Assert.Single(RegrasDeArquitetura.VerificarControllersSemAcessoADados(new[] { comRepository }));
        Assert.Single(RegrasDeArquitetura.VerificarControllersSemAcessoADados(new[] { comBanco }));
        Assert.Empty(RegrasDeArquitetura.VerificarControllersSemAcessoADados(new[] { correto }));
    }

    // ---------- R5: camada web ----------

    [Fact]
    public void R5_ServiceComTipoDaCamadaWeb_DeveSerDetectado()
    {
        var service = Cs("Modules/Estoque/Services/EstoqueService.cs", "class E { void M(IFormFile f) { } }");
        var repository = Cs("Modules/Estoque/Repositories/ProdutoRepository.cs", "class R { void M() { var t = TempData; } }");
        var controller = Cs("Modules/Estoque/Controllers/EstoqueController.cs", "class C { void M(IFormFile f) { } }");

        Assert.Single(RegrasDeArquitetura.VerificarServicosSemCamadaWeb(new[] { service }));
        Assert.Single(RegrasDeArquitetura.VerificarServicosSemCamadaWeb(new[] { repository }));
        Assert.Empty(RegrasDeArquitetura.VerificarServicosSemCamadaWeb(new[] { controller })); // controllers podem
    }

    // ---------- R6: models ----------

    [Fact]
    public void R6_ModelDependendoDeService_DeveSerDetectado()
    {
        var model = Cs("Modules/Vendas/Models/VendaModel.cs", "using PolarDrinks.Modules.Vendas.Services;\nclass V { }");
        var correto = Cs("Modules/Vendas/Models/ItemVendaModel.cs", "using PolarDrinks.Modules.Estoque.Models;\nclass I { }");

        Assert.Single(RegrasDeArquitetura.VerificarModelsIndependentes(new[] { model }));
        Assert.Empty(RegrasDeArquitetura.VerificarModelsIndependentes(new[] { correto }));
    }

    // ---------- R7: compartilhado ----------

    [Fact]
    public void R7_CompartilhadoDependendoDeModulo_DeveSerDetectado()
    {
        var arquivo = Cs("Compartilhado/Common/X.cs", "using PolarDrinks.Modules.Estoque.Models;\nclass X { }");

        Assert.Single(RegrasDeArquitetura.VerificarCompartilhadoSemModulos(new[] { arquivo }));
    }

    // ---------- R8: namespace ----------

    [Fact]
    public void R8_NamespaceDiferenteDaPasta_DeveSerDetectado()
    {
        var errado = Cs("Modules/Vendas/Services/X.cs", "namespace PolarDrinks.Modules.Estoque.Services;\nclass X { }");
        var certo = Cs("Modules/Vendas/Services/Y.cs", "namespace PolarDrinks.Modules.Vendas.Services;\nclass Y { }");
        var comSubpasta = Cs("Modules/Loja/Controllers/Api/Z.cs", "namespace PolarDrinks.Modules.Loja.Controllers.Api\n{\n class Z { }\n}");

        Assert.Single(RegrasDeArquitetura.VerificarNamespaceIgualAPasta(new[] { errado }));
        Assert.Empty(RegrasDeArquitetura.VerificarNamespaceIgualAPasta(new[] { certo }));
        Assert.Empty(RegrasDeArquitetura.VerificarNamespaceIgualAPasta(new[] { comSubpasta }));
    }

    // ---------- R9: views ----------

    [Fact]
    public void R9_PastaDeViewsSemControllerNoMesmoModulo_DeveSerDetectada()
    {
        var semController = new[]
        {
            Cs("Modules/Vendas/Views/Foo/Index.cshtml", "<h1>Foo</h1>"),
            Cs("Modules/Vendas/Controllers/VendaController.cs", "class V { }")
        };

        var comController = new[]
        {
            Cs("Modules/Vendas/Views/Venda/Index.cshtml", "<h1>Venda</h1>"),
            Cs("Modules/Vendas/Controllers/VendaController.cs", "class V { }")
        };

        var controllerEmOutroModulo = new[]
        {
            Cs("Modules/Vendas/Views/Estoque/Index.cshtml", "<h1>Estoque</h1>"),
            Cs("Modules/Estoque/Controllers/EstoqueController.cs", "class E { }")
        };

        Assert.Single(RegrasDeArquitetura.VerificarViewsNoModuloDoController(semController));
        Assert.Empty(RegrasDeArquitetura.VerificarViewsNoModuloDoController(comController));
        Assert.Single(RegrasDeArquitetura.VerificarViewsNoModuloDoController(controllerEmOutroModulo));
    }
}
