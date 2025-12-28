using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly Projet__E_commerce.Data.ApplicationDbContext _db;

        public AccountController(Projet__E_commerce.Data.ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> UserDashboard()
        {
            // Vérifier si l'utilisateur est Client
            if (HttpContext.Session.GetString("UserRole") != "CLIENT")
            {
                return RedirectToAction("Login", "Auth");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Fetch orders
            var orders = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                _db.Commandes
                .Include(c => c.LignesCommande)
                .Where(c => c.idClient == userId)
                .OrderByDescending(c => c.created_at)
            );

            // Map to ViewModel
            var model = orders.Select(o => new Projet__E_commerce.Models.OrderViewModel
            {
                Id = "CMD-" + o.idCommande.ToString("D5"),
                Date = o.created_at.ToString("dd MMM yyyy"),
                Status = o.statut,
                Total = o.prixTotal,
                Items = o.LignesCommande.Sum(l => l.quantite),
                StatusClass = GetStatusClass(o.statut)
            }).ToList();

            return View(model);
        }

        private string GetStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "livre" => "bg-success-subtle text-success",
                "valide" => "bg-info-subtle text-info",
                "en_preparation" => "bg-warning-subtle text-warning",
                "en_livraison" => "bg-primary-subtle text-primary",
                "en_attente" => "bg-secondary-subtle text-secondary",
                "annule" => "bg-danger-subtle text-danger",
                _ => "bg-light text-muted"
            };
        }
    }
}
