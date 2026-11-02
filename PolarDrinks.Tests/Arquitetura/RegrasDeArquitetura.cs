using System.Text.RegularExpressions;

namespace PolarDrinks.Tests.Arquitetura;

/// <summary>
/// Um arquivo do projeto: caminho relativo à pasta PolarDrinks/ (sempre com "/") e o texto.
/// </summary>
public sealed record ArquivoFonte(string Caminho, string Texto);

/// <summary>
/// Regras de arquitetura do PolarDrinks, verificadas lendo o código-fonte.
/// Cada método devolve a lista de violações encontradas (lista vazia = regra cumprida).
/// Comentários e textos entre aspas são ignorados.
/// </summary>
public static class RegrasDeArquitetura
{
    public static readonly string[] Modulos = { "Usuarios", "Estoque", "Compras", "Vendas", "Dashboard", "Loja" };

    /// <summary>
    /// Quem pode depender de quem. Um módulo sempre pode usar o próprio código e o Compartilhado.
    /// Compras, Vendas e Loja dependem do Estoque (preço, produtos e saldo);
    /// o Dashboard só LÊ de Estoque, Vendas e Loja; o Usuarios não depende de ninguém.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> DependenciasPermitidas =
        new Dictionary<string, string[]>
        {
            ["Usuarios"] = Array.Empty<string>(),
            ["Estoque"] = new[] { "Usuarios" },
            ["Compras"] = new[] { "Estoque", "Usuarios" },
            ["Vendas"] = new[] { "Estoque", "Usuarios" },
            ["Dashboard"] = new[] { "Estoque", "Vendas", "Loja", "Usuarios" },
            ["Loja"] = new[] { "Estoque", "Usuarios" }
        };

    /// <summary>
    /// EXCEÇÃO DOCUMENTADA: a movimentação de estoque guarda a referência ao item de origem
    /// (venda, compra ou pedido) para a tela de movimentações mostrar de onde veio o lançamento.
    /// É rastreabilidade, e só este arquivo pode apontar do Estoque para esses módulos.
    /// </summary>
    public const string ArquivoDeRastreabilidade = "Modules/Estoque/Models/MovimentacaoEstoqueModel.cs";
    public static readonly string[] ModulosDeOrigemDaRastreabilidade = { "Compras", "Vendas", "Loja" };

    // ------------------------------------------------------------------ utilidades

    public static string? ModuloDe(string caminho)
    {
        var m = Regex.Match(caminho, @"^Modules/(\w+)/");
        if (m.Success) return m.Groups[1].Value;
        return caminho.StartsWith("Compartilhado/", StringComparison.Ordinal) ? "Compartilhado" : null;
    }

    public static string? CamadaDe(string caminho)
    {
        var m = Regex.Match(caminho, @"^(?:Modules/\w+|Compartilhado)/(\w+)/");
        return m.Success ? m.Groups[1].Value : null;
    }

    /// <summary>Remove comentários e o conteúdo de textos entre aspas, para não gerar falsos alarmes.</summary>
    public static string LimparCodigo(string texto)
    {
        texto = Regex.Replace(texto, @"/\*.*?\*/", "", RegexOptions.Singleline);
        texto = Regex.Replace(texto, @"//[^\n]*", "");
        texto = Regex.Replace(texto, @"@""(?:[^""]|"""")*""", "\"\"");
        texto = Regex.Replace(texto, @"""(?:\\.|[^""\\\n])*""", "\"\"");
        return texto;
    }

    private static IEnumerable<ArquivoFonte> ArquivosCs(IEnumerable<ArquivoFonte> arquivos) =>
        arquivos.Where(a => a.Caminho.EndsWith(".cs", StringComparison.Ordinal));

    private static List<string> Procurar(
        IEnumerable<ArquivoFonte> arquivos, Func<ArquivoFonte, bool> seAplica, Regex proibido, string motivo)
    {
        var violacoes = new List<string>();

        foreach (var arquivo in ArquivosCs(arquivos).Where(seAplica))
        {
            foreach (var achado in proibido.Matches(LimparCodigo(arquivo.Texto)).Select(m => m.Value).Distinct())
            {
                violacoes.Add($"{arquivo.Caminho}: usa '{achado}' ({motivo})");
            }
        }

        return violacoes;
    }

    // ------------------------------------------------------------------ regras

    /// <summary>R1: os módulos só dependem uns dos outros nas direções permitidas.</summary>
    public static List<string> VerificarDependenciasEntreModulos(IEnumerable<ArquivoFonte> arquivos)
    {
        var violacoes = new List<string>();
        var citacao = new Regex(@"PolarDrinks\.Modules\.(\w+)");

        foreach (var arquivo in ArquivosCs(arquivos))
        {
            var modulo = ModuloDe(arquivo.Caminho);

            if (modulo == null || modulo == "Compartilhado")
            {
                continue;
            }

            if (!DependenciasPermitidas.TryGetValue(modulo, out var permitidos))
            {
                violacoes.Add($"{arquivo.Caminho}: o módulo '{modulo}' não está declarado em DependenciasPermitidas");
                continue;
            }

            var liberados = permitidos.ToList();

            if (arquivo.Caminho == ArquivoDeRastreabilidade)
            {
                liberados.AddRange(ModulosDeOrigemDaRastreabilidade);
            }

            var referenciados = citacao.Matches(LimparCodigo(arquivo.Texto))
                .Select(m => m.Groups[1].Value)
                .Distinct();

            foreach (var referenciado in referenciados)
            {
                if (referenciado == modulo || liberados.Contains(referenciado))
                {
                    continue;
                }

                violacoes.Add($"{arquivo.Caminho}: o módulo {modulo} não pode depender do módulo {referenciado}");
            }
        }

        return violacoes;
    }

    /// <summary>R2: só o Estoque usa o repository de movimentações.</summary>
    public static List<string> VerificarMovimentacoesSoNoEstoque(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => ModuloDe(a.Caminho) is { } m && m != "Estoque",
            new Regex(@"\bI?MovimentacaoEstoqueRepository\b"),
            "só o módulo Estoque acessa as movimentações");

    /// <summary>R3: só o Estoque altera o saldo (ProdutoQtdEstoque). Ler e comparar é permitido.</summary>
    public static List<string> VerificarSaldoSoAlteradoPeloEstoque(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => ModuloDe(a.Caminho) is { } m && m != "Estoque",
            new Regex(@"\bProdutoQtdEstoque\s*(?:\+=|-=|=(?!=))"),
            "só o módulo Estoque altera o saldo");

    /// <summary>R4: controllers não acessam repositórios nem o banco; passam pelos services.</summary>
    public static List<string> VerificarControllersSemAcessoADados(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => CamadaDe(a.Caminho) == "Controllers",
            new Regex(@"\b(?:I?\w+Repository|ApplicationDbContext|I?UnitOfWork)\b|PolarDrinks\.(?:Data\b|[\w.]*\.Repositories\b)"),
            "controllers falam com services, não com dados");

    /// <summary>R5: services e repositories não usam tipos da camada web (MVC/HTTP).</summary>
    public static List<string> VerificarServicosSemCamadaWeb(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => CamadaDe(a.Caminho) is "Services" or "Repositories",
            new Regex(@"\b(?:IFormFile|HttpContext|IHttpContextAccessor|IActionResult|ControllerBase|Controller|ViewBag|ViewData|TempData|SelectListItem|ModelStateDictionary)\b|Microsoft\.AspNetCore\.Mvc\b"),
            "services e repositories não conhecem a camada web");

    /// <summary>R6: models não dependem de services, repositories, controllers, filtros nem do banco.</summary>
    public static List<string> VerificarModelsIndependentes(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => CamadaDe(a.Caminho) == "Models",
            new Regex(@"PolarDrinks\.(?:Data\b|[\w.]*\.(?:Services|Repositories|Controllers|Filters)\b)"),
            "models são a base: não dependem das camadas de cima");

    /// <summary>R7: o Compartilhado não depende de nenhum módulo.</summary>
    public static List<string> VerificarCompartilhadoSemModulos(IEnumerable<ArquivoFonte> arquivos) =>
        Procurar(
            arquivos,
            a => ModuloDe(a.Caminho) == "Compartilhado",
            new Regex(@"PolarDrinks\.Modules\b"),
            "o Compartilhado não depende de módulos");

    /// <summary>R8: o namespace de cada arquivo é igual ao caminho da sua pasta.</summary>
    public static List<string> VerificarNamespaceIgualAPasta(IEnumerable<ArquivoFonte> arquivos)
    {
        var violacoes = new List<string>();

        foreach (var arquivo in ArquivosCs(arquivos).Where(a => ModuloDe(a.Caminho) != null))
        {
            var pasta = arquivo.Caminho[..arquivo.Caminho.LastIndexOf('/')];
            var esperado = "PolarDrinks." + pasta.Replace('/', '.');

            var declarado = Regex.Match(arquivo.Texto, @"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline);

            if (!declarado.Success)
            {
                violacoes.Add($"{arquivo.Caminho}: sem declaração de namespace (esperado {esperado})");
            }
            else if (declarado.Groups[1].Value != esperado)
            {
                violacoes.Add($"{arquivo.Caminho}: namespace '{declarado.Groups[1].Value}' deveria ser '{esperado}'");
            }
        }

        return violacoes;
    }

    /// <summary>R9: toda pasta de views de um módulo tem o controller correspondente no mesmo módulo.</summary>
    public static List<string> VerificarViewsNoModuloDoController(IEnumerable<ArquivoFonte> arquivos)
    {
        var lista = arquivos.ToList();
        var caminhosCs = ArquivosCs(lista).Select(a => a.Caminho).ToList();
        var violacoes = new List<string>();

        var pastasDeViews = lista
            .Select(a => Regex.Match(a.Caminho, @"^Modules/(\w+)/Views/(\w+)/"))
            .Where(m => m.Success)
            .Select(m => (Modulo: m.Groups[1].Value, Pasta: m.Groups[2].Value))
            .Distinct();

        foreach (var (modulo, pasta) in pastasDeViews)
        {
            var padrao = new Regex($@"^Modules/{modulo}/Controllers/(?:\w+/)*{pasta}Controller\.cs$");

            if (!caminhosCs.Any(c => padrao.IsMatch(c)))
            {
                violacoes.Add($"Modules/{modulo}/Views/{pasta}: não existe {pasta}Controller no módulo {modulo}");
            }
        }

        return violacoes;
    }
}

/// <summary>Lê os arquivos do projeto PolarDrinks a partir da pasta onde o teste está rodando.</summary>
public static class CodigoFonte
{
    public static string PastaDoProjeto()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);

        while (pasta != null)
        {
            var candidato = Path.Combine(pasta.FullName, "PolarDrinks");

            if (File.Exists(Path.Combine(candidato, "PolarDrinks.csproj")))
            {
                return candidato;
            }

            pasta = pasta.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Não encontrei a pasta PolarDrinks (com PolarDrinks.csproj) subindo a partir de '{AppContext.BaseDirectory}'. " +
            "Os testes de arquitetura leem o código-fonte e precisam rodar dentro do repositório.");
    }

    public static List<ArquivoFonte> Carregar() => Carregar(PastaDoProjeto());

    public static List<ArquivoFonte> Carregar(string pastaDoProjeto)
    {
        return Directory.EnumerateFiles(pastaDoProjeto, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal))
            .Select(f => (Relativo: Path.GetRelativePath(pastaDoProjeto, f).Replace('\\', '/'), Completo: f))
            .Where(x => !Regex.IsMatch(x.Relativo, @"^(?:bin|obj|Migrations|wwwroot)/"))
            .Select(x => new ArquivoFonte(x.Relativo, File.ReadAllText(x.Completo)))
            .ToList();
    }
}
