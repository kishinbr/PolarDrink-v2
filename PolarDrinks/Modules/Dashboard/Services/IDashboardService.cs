using PolarDrinks.Modules.Dashboard.Models;

namespace PolarDrinks.Modules.Dashboard.Services
{
    public interface IDashboardService
    {
        DashboardViewModel GerarDashboard();
    }
}