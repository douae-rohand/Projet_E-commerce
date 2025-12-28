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

        [HttpPost("Account/AddAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AdresseLivraison adresse)
        {
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
                adresse.idClient = userId.Value;
                adresse.created_at = DateTime.Now;

                // Si c'est la première adresse, on la met par défaut
                var hasAddresses = await _db.AdressesLivraison.AnyAsync(a => a.idClient == userId.Value);
                if (!hasAddresses)
                {
                    adresse.est_par_defaut = true;
                }
                else if (adresse.est_par_defaut)
                {
                    // Désactiver les autres adresses par défaut
                    var currentDefaults = await _db.AdressesLivraison
                        .Where(a => a.idClient == userId.Value && a.est_par_defaut)
                        .ToListAsync();
                    foreach (var d in currentDefaults)
                    {
                        d.est_par_defaut = false;
                    }
                }

                _db.AdressesLivraison.Add(adresse);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "L'adresse a été ajoutée avec succès.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue : " + ex.Message;
            }

            return RedirectToAction("UserDashboard");
        }

        [HttpPost("Account/UpdateAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(int idAdresse, string nom_adresse, string adresse_complete, string ville, string code_postal, string telephone, bool est_par_defaut)
        {
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
                var adresse = await _db.AdressesLivraison.FirstOrDefaultAsync(a => a.idAdresse == idAdresse && a.idClient == userId.Value);
                if (adresse == null)
                {
                    TempData["ErrorMessage"] = "Adresse non trouvée.";
                    return RedirectToAction("UserDashboard");
                }

                adresse.nom_adresse = nom_adresse?.Trim();
                adresse.adresse_complete = adresse_complete?.Trim();
                adresse.ville = ville?.Trim();
                adresse.code_postal = code_postal?.Trim();
                adresse.telephone = telephone?.Trim();

                if (est_par_defaut && !adresse.est_par_defaut)
                {
                    // Désactiver les autres adresses par défaut
                    var currentDefaults = await _db.AdressesLivraison
                        .Where(a => a.idClient == userId.Value && a.est_par_defaut)
                        .ToListAsync();
                    foreach (var d in currentDefaults)
                    {
                        d.est_par_defaut = false;
                    }
                    adresse.est_par_defaut = true;
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "L'adresse a été mise à jour avec succès.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue : " + ex.Message;
            }

            return RedirectToAction("UserDashboard");
        }

        [HttpPost("Account/DeleteAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int idAdresse)
        {
            if (HttpContext.Session.GetString("UserRole") != "CLIENT")
            {
                return Json(new { success = false, message = "Non autorisé" });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Non autorisé" });
            }

            try
            {
                var adresse = await _db.AdressesLivraison.FirstOrDefaultAsync(a => a.idAdresse == idAdresse && a.idClient == userId.Value);
                if (adresse == null)
                {
                    return Json(new { success = false, message = "Adresse non trouvée" });
                }

                _db.AdressesLivraison.Remove(adresse);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "L'adresse a été supprimée avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Une erreur est survenue : " + ex.Message });
            }
        }

        [HttpGet("Account/OrderDetails/{id}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var order = await _db.Commandes
                .Include(o => o.LignesCommande)
                    .ThenInclude(lc => lc.Variante)
                        .ThenInclude(v => v.Produit)
                .Include(o => o.Livraison)
                    .ThenInclude(l => l.AdresseLivraison)
                .FirstOrDefaultAsync(o => o.idCommande == id && o.idClient == userId.Value);

            if (order == null) return NotFound();

            return PartialView("_OrderDetailsPartial", order);
        }
    }
}
