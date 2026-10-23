using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Dashboard.Models;

namespace PolarDrinks.Modules.Dashboard.Services
{
    public interface IRelatorioService
    {
        ResultadoOperacao<ResumoVendasDto> GerarResumoPorPeriodo(string? canal, DateTime? inicio, DateTime? fim);
    }
}