using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Modules.Dashboard.Models;

namespace PolarDrinks.Modules.Dashboard.Repositories
{
    /// <summary>
    /// Consulta de leitura do relatório de vendas: escolhe a procedure de resumo conforme o canal.
    /// As procedures são criadas pelas migrations AdicionarTriggersEProcedures e AdicionarProceduresOnlineEGeral.
    /// </summary>
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly ApplicationDbContext _db;

        public RelatorioRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public ResumoVendasDto ObterResumoPorPeriodo(string canal, DateTime inicio, DateTime fim)
        {
            FormattableString sql;

            if (canal == "online")
            {
                sql = $"EXEC sp_ResumoPedidosOnlinePorPeriodo @Inicio = {inicio}, @Fim = {fim}";
            }
            else if (canal == "geral")
            {
                sql = $"EXEC sp_ResumoGeralPorPeriodo @Inicio = {inicio}, @Fim = {fim}";
            }
            else
            {
                sql = $"EXEC sp_ResumoVendasPorPeriodo @Inicio = {inicio}, @Fim = {fim}";
            }

            return _db.Database
                .SqlQuery<ResumoVendasDto>(sql)
                .ToList()
                .First();
        }
    }
}