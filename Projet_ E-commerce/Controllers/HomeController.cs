using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Models;
using Projet__E_commerce.Filters;
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

        // Page d'accueil - accessible à tous
        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();
            return View(stats);
        }

        // Page de confidentialité
        public IActionResult Privacy()
        {
            return View();
        }

        // Page des conditions d'utilisation
        public IActionResult Terms()
        {
            return Content("Coming soon");
        }

        // Mes commandes - accessible uniquement aux clients connectés
        [AuthorizeRole("CLIENT")]
        public IActionResult MyOrders()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        // Profil utilisateur - accessible uniquement aux clients connectés
        [AuthorizeRole("CLIENT")]
        public IActionResult Profile()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        // Page d'erreur
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}