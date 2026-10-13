using Microsoft.Data.SqlClient;

namespace PolarDrinks.Tests.Integracao;

/// <summary>
/// Igual ao [Fact], mas o teste é PULADO (com o motivo) se o SQL Server de testes não estiver acessível.
/// </summary>
public sealed class FatoIntegracaoAttribute : FactAttribute
{
    private static readonly Lazy<string?> MotivoIndisponivel = new(VerificarServidor);

    public FatoIntegracaoAttribute()
    {
        Skip = MotivoIndisponivel.Value;
    }

    private static string? VerificarServidor()
    {
        try
        {
            var servidor = Environment.GetEnvironmentVariable(BancoDeTestes.VariavelDeConexao)
                           ?? BancoDeTestes.ConexaoPadrao;

            var construtor = new SqlConnectionStringBuilder(servidor) { ConnectTimeout = 5 };

            using var conexao = new SqlConnection(construtor.ConnectionString);
            conexao.Open();

            return null; // servidor acessível: o teste roda
        }
        catch (Exception ex)
        {
            return $"SQL Server de testes indisponível ({ex.GetType().Name}). " +
                   $"Inicie o servidor ou defina a variável {BancoDeTestes.VariavelDeConexao}.";
        }
    }
}

/// <summary>
/// Todos os testes de integração usam a mesma coleção: compartilham UM banco temporário
/// por execução e rodam em sequência (não em paralelo).
/// </summary>
[CollectionDefinition(Nome)]
public class ColecaoBancoDeTestes : ICollectionFixture<BancoDeTestes>
{
    public const string Nome = "Banco de testes";
}