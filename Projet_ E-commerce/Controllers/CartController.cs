using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Models;
using Projet__E_commerce.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Projet__E_commerce.Controllers
{
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "auth");

            var cartItems = await _db.LignesCommande
                .Include(lc => lc.Variante)
                .ThenInclude(v => v.Produit)
                .Where(lc => lc.Commande.idClient == userId && lc.Commande.statut == "en_attente")
                .Select(lc => new CartItemViewModel
                {
                    Id = lc.Variante.idP,
                    Name = lc.Variante.Produit.nomP,
                    Price = lc.prix_unitaire,
                    Quantity = lc.quantite,
                    Size = lc.Variante.taille ?? "",
                    Color = lc.Variante.couleur ?? "",
                    Image = lc.Variante.photo ?? ""
                })
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string size, string color)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                var pendingItem = new { productId, quantity, size, color };
                HttpContext.Session.SetString("PendingCartItem", JsonSerializer.Serialize(pendingItem));
                return RedirectToAction("Login", "auth", new { returnUrl = Url.Action("ProcessPendingItem", "Cart") });
            }

            // Find or create 'en_attente' order
            var order = await _db.Commandes
                .FirstOrDefaultAsync(c => c.idClient == userId && c.statut == "en_attente");

            if (order == null)
            {
                order = new Commande
                {
                    idClient = userId.Value,
                    statut = "en_attente",
                    prixTotal = 0,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                _db.Commandes.Add(order);
                await _db.SaveChangesAsync();
            }

            // Find specific variant or first available
            var variantQuery = _db.Variantes.Where(v => v.idP == productId);
            
            if (!string.IsNullOrEmpty(size))
                variantQuery = variantQuery.Where(v => v.taille == size);
            
            if (!string.IsNullOrEmpty(color))
                variantQuery = variantQuery.Where(v => v.couleur == color);

            var variant = await variantQuery.FirstOrDefaultAsync();

            if (variant == null)
            {
                // Fallback: just get the first variant for this product
                variant = await _db.Variantes.FirstOrDefaultAsync(v => v.idP == productId);
            }

            if (variant == null) return NotFound("Produit ou variante non trouvé.");

            // Check if item already exists in the order
            var existingLine = await _db.LignesCommande
                .FirstOrDefaultAsync(lc => lc.idCommande == order.idCommande && lc.idV == variant.idV);

            if (existingLine != null)
            {
                existingLine.quantite += quantity;
                _db.LignesCommande.Update(existingLine);
            }
            else
            {
                var newLine = new LigneCommande
                {
                    idCommande = order.idCommande,
                    idV = variant.idV,
                    quantite = quantity,
                    prix_unitaire = variant.prix,
                    created_at = DateTime.Now
                };
                _db.LignesCommande.Add(newLine);
            }

            // Update Total Price
            await _db.SaveChangesAsync();
            order.prixTotal = await _db.LignesCommande
                .Where(lc => lc.idCommande == order.idCommande)
                .SumAsync(lc => lc.prix_unitaire * lc.quantite);
            
            _db.Commandes.Update(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId, string size, string color)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "auth");

            var variant = await _db.Variantes
                .FirstOrDefaultAsync(v => v.idP == productId && v.taille == size && v.couleur == color);

            if (variant != null)
            {
                var order = await _db.Commandes
                    .FirstOrDefaultAsync(c => c.idClient == userId && c.statut == "en_attente");

                if (order != null)
                {
                    var line = await _db.LignesCommande
                        .FirstOrDefaultAsync(lc => lc.idCommande == order.idCommande && lc.idV == variant.idV);

                    if (line != null)
                    {
                        _db.LignesCommande.Remove(line);
                        await _db.SaveChangesAsync();

                        // Update Total Price
                        order.prixTotal = await _db.LignesCommande
                            .Where(lc => lc.idCommande == order.idCommande)
                            .SumAsync(lc => lc.prix_unitaire * lc.quantite);

                        // If no more lines, maybe delete the order? Or keep it. 
                        // Let's keep it but with total 0.
                        _db.Commandes.Update(order);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Test()
        {
            return Content("CartController is reachable!");
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPendingItem()
        {
            var pendingItemJson = HttpContext.Session.GetString("PendingCartItem");
            if (!string.IsNullOrEmpty(pendingItemJson))
            {
                var item = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(pendingItemJson);
                if (item != null)
                {
                    int productId = item["productId"].GetInt32();
                    int quantity = item["quantity"].GetInt32();
                    string size = item["size"].GetString() ?? "";
                    string color = item["color"].GetString() ?? "";

                    HttpContext.Session.Remove("PendingCartItem");
                    return await AddToCart(productId, quantity, size, color);
                }
            }
            return RedirectToAction("Index");
        }

        // Clean up legacy session methods
        private void SaveCartToSession(List<CartItemViewModel> cart) { }
        private List<CartItemViewModel> GetCartFromSession() { return new List<CartItemViewModel>(); }
    }
}
