using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
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

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = await _db.Clients
                .Include(c => c.Utilisateur)
                .Include(c => c.Commandes)
                    .ThenInclude(o => o.LignesCommande)
                .Include(c => c.AdressesLivraison)
                .FirstOrDefaultAsync(c => c.id == userId.Value);

            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(client);
        }

        [HttpPost("Account/UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string prenom, string nom, string telephone)
        {
            // Vérifier si l'utilisateur est Client
            if (HttpContext.Session.GetString("UserRole") != "CLIENT")
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = await _db.Clients.FirstOrDefaultAsync(c => c.id == userId.Value);
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Client non trouvé.";
                    return RedirectToAction("UserDashboard");
                }

                // Mise à jour des champs
                client.prenom = prenom?.Trim();
                client.nom = nom?.Trim();
                client.telephone = telephone?.Trim();
                client.updated_at = DateTime.Now;

                _db.Entry(client).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Votre profil a été mis à jour avec succès.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour : " + ex.Message;
            }

            return RedirectToAction("UserDashboard");
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
