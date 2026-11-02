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
        private readonly IRelatorioService _relatorioService;

        public DashboardController(IDashboardService dashboardService, IRelatorioService relatorioService)
        {
            _dashboardService = dashboardService;
            _relatorioService = relatorioService;
        }

        public IActionResult Index()
        {
            var model = _dashboardService.GerarDashboard();
            return View(model);
        }

        public IActionResult RelatorioVendas(string? canal, DateTime? inicio, DateTime? fim)
        {
            ViewBag.Canal = canal;
            ViewBag.Inicio = inicio;
            ViewBag.Fim = fim;

            var resultado = _relatorioService.GerarResumoPorPeriodo(canal, inicio, fim);

            if (!resultado.Sucesso)
            {
                ViewBag.Erro = resultado.Mensagem;
                return View();
            }

            return View(resultado.Dado);
        }
    }
}