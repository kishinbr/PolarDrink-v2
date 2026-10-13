using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;

namespace PolarDrinks.Tests.Integracao;

/// <summary>
/// Banco SQL Server temporário para os testes de integração.
/// Cria um banco novo (PolaDrinks_Testes_xxx) com as migrations reais, e o apaga ao final.
/// Nunca toca no banco de uso cotidiano.
/// </summary>
public sealed class BancoDeTestes : IDisposable
{
    public const string VariavelDeConexao = "POLARDRINKS_TESTES_CONEXAO";
    public const string ConexaoPadrao = "Server=.;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _nomeDoBanco = $"PolaDrinks_Testes_{Guid.NewGuid():N}";

    public string Conexao { get; }
    public bool Disponivel { get; }
    public string? MotivoIndisponivel { get; }

    public BancoDeTestes()
    {
        var servidor = Environment.GetEnvironmentVariable(VariavelDeConexao) ?? ConexaoPadrao;

        var construtor = new SqlConnectionStringBuilder(servidor)
        {
            InitialCatalog = _nomeDoBanco,
            ConnectTimeout = 5
        };

        Conexao = construtor.ConnectionString;

        try
        {
            using var db = CriarContexto();
            db.Database.Migrate();
            Disponivel = true;
        }
        catch (Exception ex)
        {
            Disponivel = false;
            MotivoIndisponivel = ex.Message;
        }
    }

    public ApplicationDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(Conexao)
            .Options;

        return new ApplicationDbContext(opcoes);
    }

    /// <summary>
    /// Apaga os dados de todas as tabelas do modelo (menos o histórico de migrations),
    /// para cada cenário começar do zero.
    /// </summary>
    public void LimparDados()
    {
        using var db = CriarContexto();

        var tabelas = db.Model.GetEntityTypes()
            .Where(t => t.GetTableName() != null)
            .Select(t => $"[{t.GetSchema() ?? "dbo"}].[{t.GetTableName()}]")
            .Distinct()
            .ToList();

        foreach (var tabela in tabelas)
        {
            var sql = $"ALTER TABLE {tabela} NOCHECK CONSTRAINT ALL";
            db.Database.ExecuteSqlRaw(sql);
        }

        foreach (var tabela in tabelas)
        {
            var sql = $"DELETE FROM {tabela}";
            db.Database.ExecuteSqlRaw(sql);
        }

        foreach (var tabela in tabelas)
        {
            var sql = $"ALTER TABLE {tabela} WITH CHECK CHECK CONSTRAINT ALL";
            db.Database.ExecuteSqlRaw(sql);
        }
    }

    public void Dispose()
    {
        if (!Disponivel)
        {
            return;
        }

        try
        {
            using var db = CriarContexto();
            db.Database.EnsureDeleted();
        }
        catch
        {
            // Melhor esforço: se não conseguir apagar, o banco temporário fica no servidor.
        }
    }
}