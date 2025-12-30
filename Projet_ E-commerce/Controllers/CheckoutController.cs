using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Models;
using System.Text.Json;

namespace Projet__E_commerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const string CartSessionKey = "Cart";

        public CheckoutController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Checkout" });
            }

            // Load the 'en_attente' order (the cart)
            var order = await _db.Commandes
                .Include(c => c.LignesCommande)
                .ThenInclude(lc => lc.Variante)
                .ThenInclude(v => v.Produit)
                .FirstOrDefaultAsync(c => c.idClient == userId && c.statut == "en_attente");

            if (order == null || !order.LignesCommande.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = order.LignesCommande.Select(lc => new CartItemViewModel
            {
                ProductId = lc.Variante.idP,
                VariantId = lc.idV,
                Name = lc.Variante.Produit.nomP,
                Price = lc.prix_unitaire,
                Quantity = lc.quantite,
                Size = lc.Variante.taille ?? "",
                Color = lc.Variante.couleur ?? "",
                Image = lc.Variante.photo ?? ""
            }).ToList();

            // Validate Stock in Index too
            foreach (var line in order.LignesCommande)
            {
                if (line.Variante.quantite < line.quantite)
                {
                    TempData["CartError"] = $"Stock insuffisant pour {line.Variante.Produit.nomP}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            ViewBag.CartTotal = order.prixTotal;
            ViewBag.CartItems = cartItems;

            var addresses = await _db.AdressesLivraison.Where(a => a.idClient == userId).ToListAsync();

            var model = new CheckoutViewModel
            {
                ExistingAddresses = addresses,
                SelectedAddressId = addresses.FirstOrDefault(a => a.est_par_defaut)?.idAdresse ?? addresses.FirstOrDefault()?.idAdresse
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Load the existing 'en_attente' order
            var order = await _db.Commandes
                .Include(c => c.LignesCommande)
                .ThenInclude(lc => lc.Variante)
                .ThenInclude(v => v.Produit)
                .FirstOrDefaultAsync(c => c.idClient == userId && c.statut == "en_attente");

            if (order == null || !order.LignesCommande.Any()) return RedirectToAction("Index", "Cart");

            var cartItems = order.LignesCommande.Select(lc => new CartItemViewModel
            {
                ProductId = lc.Variante.idP,
                VariantId = lc.idV,
                Name = lc.Variante.Produit.nomP,
                Price = lc.prix_unitaire,
                Quantity = lc.quantite,
                Size = lc.Variante.taille ?? "",
                Color = lc.Variante.couleur ?? "",
                Image = lc.Variante.photo ?? ""
            }).ToList();

            // Custom Validation: If New Address (SelectedAddressId null/0), then fields are required.
            if ((!model.SelectedAddressId.HasValue || model.SelectedAddressId == 0))
            {
                if (string.IsNullOrWhiteSpace(model.Address)) ModelState.AddModelError("Address", "L'adresse est requise");
                if (string.IsNullOrWhiteSpace(model.City)) ModelState.AddModelError("City", "La ville est requise");
                if (string.IsNullOrWhiteSpace(model.ZipCode)) ModelState.AddModelError("ZipCode", "Le code postal est requis");
                if (string.IsNullOrWhiteSpace(model.PhoneNumber)) ModelState.AddModelError("PhoneNumber", "Le téléphone est requis");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CartTotal = order.prixTotal;
                ViewBag.CartItems = cartItems;
                model.ExistingAddresses = await _db.AdressesLivraison.Where(a => a.idClient == userId).ToListAsync();
                return View("Index", model);
            }

            // Pre-Validate Stock Availability
            foreach (var line in order.LignesCommande)
            {
                if (line.Variante.quantite < line.quantite)
                {
                    ModelState.AddModelError("", $"Stock insuffisant pour {line.Variante.Produit.nomP} ({line.Variante.taille}/{line.Variante.couleur}). Disponible: {line.Variante.quantite}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CartTotal = order.prixTotal;
                ViewBag.CartItems = cartItems;
                model.ExistingAddresses = await _db.AdressesLivraison.Where(a => a.idClient == userId).ToListAsync();
                return View("Index", model);
            }

            // 1. Resolve Address
            int finalAddressId;
            if (model.SelectedAddressId.HasValue && model.SelectedAddressId > 0)
            {
                finalAddressId = model.SelectedAddressId.Value;
            }
            else
            {
                var newAddr = new AdresseLivraison
                {
                    idClient = userId.Value,
                    nom_adresse = model.NewAddressName ?? "Nouvelle adresse",
                    adresse_complete = model.Address,
                    ville = model.City,
                    code_postal = model.ZipCode,
                    telephone = model.PhoneNumber,
                    est_par_defaut = false,
                    created_at = DateTime.Now
                };
                _db.AdressesLivraison.Add(newAddr);
                await _db.SaveChangesAsync();
                finalAddressId = newAddr.idAdresse;
            }

            // 2. Finalize Order
            order.statut = "en_preparation";
            order.updated_at = DateTime.Now;

            // 3. Create Livraison
            var livraison = new Livraison
            {
                idCommande = order.idCommande,
                idAdresse = finalAddressId,
                mode_livraison = model.DeliveryMode,
                notes = "Commande e-commerce",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            if (model.DeliveryMode == "Express")
            {
                livraison.dateDebutEstimation = DateTime.Now.AddDays(1);
                livraison.dateFinEstimation = DateTime.Now.AddDays(2);
                livraison.frais = 50; 
                order.prixTotal += 50; 
            }
            else
            {
                livraison.dateDebutEstimation = DateTime.Now.AddDays(3);
                livraison.dateFinEstimation = DateTime.Now.AddDays(5);
                livraison.frais = 30;
                order.prixTotal += 30;
            }
            _db.Livraisons.Add(livraison);
            
            // 4. Deduct Stock
            foreach (var line in order.LignesCommande)
            {
                line.Variante.quantite -= line.quantite;
                _db.Variantes.Update(line.Variante);
            }
            
            await _db.SaveChangesAsync();

            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            return View();
        }

        // Clean up legacy session methods
        private List<CartItemViewModel> GetCartFromSession() { return new List<CartItemViewModel>(); }
    }
}
