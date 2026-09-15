using Microsoft.AspNetCore.Mvc;
using PolarDrinks.Filters;
using PolarDrinks.Services;

namespace PolarDrinks.Controllers
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