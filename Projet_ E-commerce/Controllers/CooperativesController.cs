using Microsoft.AspNetCore.Mvc;

namespace Projet__E_commerce.Controllers
{
    public class CooperativesController : Controller
    {
        private readonly Projet__E_commerce.Services.IStatisticsService _statisticsService;

        public CooperativesController(Projet__E_commerce.Services.IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();
            return View(stats);
        }
    }
}
