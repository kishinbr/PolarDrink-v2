using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolarDrinks.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProceduresOnlineEGeral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mesmo formato da migration AdicionarTriggersEProcedures: cada procedure dentro de EXEC(...),
            // com as aspas simples dos textos dobradas.

            // PROCEDURE: resumo dos pedidos da loja online entre duas datas (a data fim é inclusiva).
            // Mesmas regras do dashboard: contam como venda os pedidos concluídos e os expirados (não retirados).
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoPedidosOnlinePorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FimExclusivo DATETIME2 = DATEADD(DAY, 1, CAST(@Fim AS DATETIME2));

    SELECT
        COUNT(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN 1 END) AS QuantidadeVendas,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN p.PedidoValorTotal END), 0) AS Faturamento,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') THEN i.LucroPedido END), 0) AS Lucro,
        CAST(0 AS DECIMAL(18,2)) AS TotalDinheiro,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') AND p.PedidoTipoPagamento = N''Pix'' THEN p.PedidoValorTotal END), 0) AS TotalPix,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''Concluido'', N''CanceladoNaoRetirado'') AND p.PedidoTipoPagamento = N''Cartao'' THEN p.PedidoValorTotal END), 0) AS TotalCartao,
        COUNT(CASE WHEN p.PedidoStatus IN (N''CanceladoCliente'', N''CanceladoAdmin'') THEN 1 END) AS QuantidadeCanceladas,
        ISNULL(SUM(CASE WHEN p.PedidoStatus IN (N''CanceladoCliente'', N''CanceladoAdmin'') THEN p.PedidoValorTotal END), 0) AS ValorCancelado
    FROM Pedidos p
    OUTER APPLY (
        SELECT SUM((ip.ItemPedidoPreco - ip.ItemPedidoCusto) * ip.ItemPedidoQtd) AS LucroPedido
        FROM ItensPedido ip
        WHERE ip.PedidoID = p.PedidoID
    ) i
    WHERE p.PedidoData >= @Inicio
      AND p.PedidoData < @FimExclusivo;
END
')");

            // PROCEDURE: resumo geral (balcão + online). Não recalcula nada: executa as duas
            // procedures acima, guarda os resultados numa tabela temporária e devolve a soma.
            migrationBuilder.Sql(@"
EXEC(N'
CREATE OR ALTER PROCEDURE sp_ResumoGeralPorPeriodo
    @Inicio DATE,
    @Fim DATE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Resumo (
        QuantidadeVendas INT,
        Faturamento DECIMAL(18,2),
        Lucro DECIMAL(18,2),
        TotalDinheiro DECIMAL(18,2),
        TotalPix DECIMAL(18,2),
        TotalCartao DECIMAL(18,2),
        QuantidadeCanceladas INT,
        ValorCancelado DECIMAL(18,2)
    );

    INSERT INTO #Resumo EXEC sp_ResumoVendasPorPeriodo @Inicio, @Fim;
    INSERT INTO #Resumo EXEC sp_ResumoPedidosOnlinePorPeriodo @Inicio, @Fim;

    SELECT
        SUM(QuantidadeVendas) AS QuantidadeVendas,
        SUM(Faturamento) AS Faturamento,
        SUM(Lucro) AS Lucro,
        SUM(TotalDinheiro) AS TotalDinheiro,
        SUM(TotalPix) AS TotalPix,
        SUM(TotalCartao) AS TotalCartao,
        SUM(QuantidadeCanceladas) AS QuantidadeCanceladas,
        SUM(ValorCancelado) AS ValorCancelado
    FROM #Resumo;
END
')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A geral usa a online, então é removida primeiro
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ResumoGeralPorPeriodo;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ResumoPedidosOnlinePorPeriodo;");
        }
    }
}