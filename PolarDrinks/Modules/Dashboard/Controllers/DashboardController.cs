using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Compartilhado.Filters;
using PolarDrinks.Modules.Dashboard.Services;

namespace PolarDrinks.Modules.Dashboard.Controllers
{
    [AuthFilter]
    [AdminFilter]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            var model = _dashboardService.GerarDashboard();
            return View(model);
        }
    }
}