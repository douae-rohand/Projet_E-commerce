using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Services;

namespace Projet__E_commerce.Controllers
{
    public class AboutController : Controller
    {
        private readonly IStatisticsService _statisticsService;

        public AboutController(IStatisticsService statisticsService)
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
