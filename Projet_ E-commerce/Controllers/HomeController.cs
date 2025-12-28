using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Models;
using Projet__E_commerce.Filters;
using Projet__E_commerce.Services;
using Microsoft.EntityFrameworkCore;

namespace Projet__E_commerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStatisticsService _statisticsService;
        private readonly Projet__E_commerce.Data.ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IStatisticsService statisticsService, Projet__E_commerce.Data.ApplicationDbContext context)
        {
            _logger = logger;
            _statisticsService = statisticsService;
            _context = context;
        }

        // Page d'accueil - accessible à tous
        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();

            // Fetch 8 random/featured products with variants and cooperative info
            var featuredProducts = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                _context.Produits
                .Include(p => p.Variantes)
                .Include(p => p.Admin)
                .Include(p => p.Categorie)
                .Where(p => p.statut == "active")
                .OrderBy(r => Guid.NewGuid()) // Random order
                .Take(8)
            );

            stats.FeaturedProducts = featuredProducts.Select(p => new ProductViewModel
            {
                Id = p.idP,
                Name = p.nomP,
                Price = p.Variantes.FirstOrDefault()?.prix ?? 0,
                OriginalPrice = 0, // Not stored in DB currently
                Image = p.Variantes.FirstOrDefault()?.photo ?? "",
                Rating = 5.0, // Mock rating as 'Avis' might be empty
                Reviews = p.Avis?.Count ?? 0,
                Category = p.Categorie?.nom ?? "N/A",
                Cooperative = p.Admin?.nom_cooperative ?? "Coopérative",
                IsBestSeller = false,
                IsNew = (DateTime.Now - p.created_at).TotalDays < 30
            }).ToList();

            return View(stats);
        }

        // Page de tous les avis (4+ étoiles)
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _statisticsService.GetTopReviewsAsync();
            return View(reviews);
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