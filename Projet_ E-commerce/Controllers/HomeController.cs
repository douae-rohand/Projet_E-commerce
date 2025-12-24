using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Models;
using Projet__E_commerce.Services;

namespace Projet__E_commerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStatisticsService _statisticsService;

        public HomeController(ILogger<HomeController> logger, IStatisticsService statisticsService)
        {
            _logger = logger;
            _statisticsService = statisticsService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();
            return View(stats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
