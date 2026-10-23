using PolarDrinks.Modules.Dashboard.Models;

namespace PolarDrinks.Modules.Dashboard.Repositories
{
    public interface IRelatorioRepository
    {
        ResumoVendasDto ObterResumoPorPeriodo(string canal, DateTime inicio, DateTime fim);
    }
}