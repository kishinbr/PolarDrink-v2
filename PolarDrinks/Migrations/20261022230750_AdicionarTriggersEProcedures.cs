using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolarDrinks.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTriggersEProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cada objeto vai dentro de EXEC(...): CREATE TRIGGER e CREATE PROCEDURE precisam ser
            // o primeiro comando do lote. Assim funciona tanto ao iniciar a aplicação (Migrate)
            // quanto no script SQL gerado pelo dotnet ef.

            // TRIGGER 1: o sistema nunca pode ficar sem um administrador ativo.
            // Protege também contra alterações feitas direto no banco, fora do sistema.
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER TRIGGER TR_Usuarios_ProtegerUltimoAdmin
ON Usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted WHERE UsuarioAtivo = 1 AND UsuarioPerfil = N''Admin'')
       AND NOT EXISTS (SELECT 1 FROM Usuarios WHERE UsuarioAtivo = 1 AND UsuarioPerfil = N''Admin'')
    BEGIN
        THROW 50001, N''O sistema precisa ter pelo menos um administrador ativo.'', 1;
    END
END
')");

            // TRIGGER 2: o saldo de um produto nunca pode ficar negativo.
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER TRIGGER TR_Produtos_ImpedirEstoqueNegativo
ON Produtos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE ProdutoQtdEstoque < 0)
    BEGIN
        THROW 50002, N''O estoque de um produto não pode ficar negativo.'', 1;
    END
END
')");

            // PROCEDURE 1: lista de reposição (produtos ativos no estoque mínimo ou abaixo dele).
            // Mesmo critério do alerta "abaixo do mínimo" do dashboard.
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER PROCEDURE sp_ProdutosAbaixoDoMinimo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.ProdutoID,
        p.ProdutoNome,
        p.ProdutoCodBarra,
        p.ProdutoQtdEstoque,
        p.ProdutoEstoqueMinimo,
        CASE
            WHEN p.ProdutoEstoqueMinimo > p.ProdutoQtdEstoque
                THEN p.ProdutoEstoqueMinimo - p.ProdutoQtdEstoque
            ELSE 0
        END AS QuantidadeFaltante,
        p.ProdutoPrecoCusto
    FROM Produtos p
    WHERE p.ProdutoAtivo = 1
      AND p.ProdutoQtdEstoque <= p.ProdutoEstoqueMinimo
    ORDER BY p.ProdutoQtdEstoque, p.ProdutoNome;
END
')");

            // PROCEDURE 2: resumo das vendas do balcão entre duas datas (a data fim é inclusiva).
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoVendasPorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FimExclusivo DATETIME2 = DATEADD(DAY, 1, CAST(@Fim AS DATETIME2));

    SELECT
        COUNT(CASE WHEN v.VendaCancelada = 0 THEN 1 END) AS QuantidadeVendas,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 THEN v.VendaValorTotal END), 0) AS Faturamento,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 THEN i.LucroVenda END), 0) AS Lucro,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Dinheiro'' THEN v.VendaValorTotal END), 0) AS TotalDinheiro,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Pix'' THEN v.VendaValorTotal END), 0) AS TotalPix,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 0 AND v.VendaTipoPagamento = N''Cartão'' THEN v.VendaValorTotal END), 0) AS TotalCartao,
        COUNT(CASE WHEN v.VendaCancelada = 1 THEN 1 END) AS QuantidadeCanceladas,
        ISNULL(SUM(CASE WHEN v.VendaCancelada = 1 THEN v.VendaValorTotal END), 0) AS ValorCancelado
    FROM Vendas v
    OUTER APPLY (
        SELECT SUM((iv.ItemVendaPreco - iv.ItemVendaCusto) * iv.ItemVendaQtd) AS LucroVenda
        FROM ItensVenda iv
        WHERE iv.VendaID = v.VendaID
    ) i
    WHERE v.VendaData >= @Inicio
      AND v.VendaData < @FimExclusivo;
END
')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ResumoVendasPorPeriodo;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ProdutosAbaixoDoMinimo;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Produtos_ImpedirEstoqueNegativo;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Usuarios_ProtegerUltimoAdmin;");
        }
    }
}