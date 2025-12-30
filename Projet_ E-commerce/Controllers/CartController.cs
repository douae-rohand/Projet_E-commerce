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
                    ProductId = lc.Variante.idP,
                    VariantId = lc.idV,
                    Name = lc.Variante.Produit.nomP,
                    Price = lc.prix_unitaire,
                    Quantity = lc.quantite,
                    Size = lc.Variante.taille ?? "",
                    Color = lc.Variante.couleur ?? "",
                    Weight = lc.Variante.poids ?? "",
                    Image = lc.Variante.photo ?? ""
                })
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int variantId, int quantity)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                var pendingItem = new { productId, variantId, quantity };
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

            // Find specific variant
            var variant = await _db.Variantes
                .Include(v => v.Produit)
                .FirstOrDefaultAsync(v => v.idV == variantId && v.idP == productId);

            if (variant == null) return NotFound("Produit ou variante non trouvé.");
            
            // Stock Validation
            int currentQtyInCart = await _db.LignesCommande
                .Where(lc => lc.idCommande == (order != null ? order.idCommande : 0) && lc.idV == variant.idV)
                .Select(lc => (int?)lc.quantite)
                .FirstOrDefaultAsync() ?? 0;

            if (variant.quantite < (currentQtyInCart + quantity))
            {
                TempData["CartError"] = $"Stock insuffisant. Disponible: {variant.quantite}";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

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
        public async Task<IActionResult> UpdateQuantity(int variantId, int change)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "auth");

            var variant = await _db.Variantes.Include(v => v.Produit).FirstOrDefaultAsync(v => v.idV == variantId);
            if (variant == null) return NotFound();

            var order = await _db.Commandes
                .FirstOrDefaultAsync(c => c.idClient == userId && c.statut == "en_attente");

            if (order != null)
            {
                var line = await _db.LignesCommande
                    .FirstOrDefaultAsync(lc => lc.idCommande == order.idCommande && lc.idV == variant.idV);

                if (line != null)
                {
                    if (change > 0 && line.Variante.quantite < (line.quantite + change))
                    {
                        TempData["CartError"] = $"Stock insuffisant pour {line.Variante.Produit.nomP}.";
                        return RedirectToAction("Index");
                    }

                    line.quantite += change;
                    if (line.quantite <= 0)
                    {
                        _db.LignesCommande.Remove(line);
                    }
                    else
                    {
                        _db.LignesCommande.Update(line);
                    }
                    await _db.SaveChangesAsync();

                    // Update Total Price
                    order.prixTotal = await _db.LignesCommande
                        .Where(lc => lc.idCommande == order.idCommande)
                        .SumAsync(lc => lc.prix_unitaire * lc.quantite);

                    _db.Commandes.Update(order);
                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int variantId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "auth");

            var variant = await _db.Variantes.FirstOrDefaultAsync(v => v.idV == variantId);
            if (variant == null) return NotFound();

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

                    order.prixTotal = await _db.LignesCommande
                        .Where(lc => lc.idCommande == order.idCommande)
                        .SumAsync(lc => lc.prix_unitaire * lc.quantite);

                    _db.Commandes.Update(order);
                    await _db.SaveChangesAsync();
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
                    int variantId = item["variantId"].GetInt32();
                    int quantity = item["quantity"].GetInt32();

                    HttpContext.Session.Remove("PendingCartItem");
                    return await AddToCart(productId, variantId, quantity);
                }
            }
            return RedirectToAction("Index");
        }

        // Clean up legacy session methods
        private void SaveCartToSession(List<CartItemViewModel> cart) { }
        private List<CartItemViewModel> GetCartFromSession() { return new List<CartItemViewModel>(); }
    }
}
