using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CatalogController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? category, string? priceRange, bool? inStock, bool? isPromo, bool? isNew)
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.nom)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            var query = _db.Produits
                .AsNoTracking()
                .Where(p => p.statut == "active")
                .Include(p => p.Variantes)
                .Include(p => p.Categorie)
                .Include(p => p.Admin)
                .Include(p => p.Avis)
                .AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(p => p.idC == category.Value);
            }

            // Price Filtering
            if (!string.IsNullOrEmpty(priceRange))
            {
                // Expected format: "min-max" or "min+"
                if (priceRange.Contains("-"))
                {
                    var parts = priceRange.Split('-');
                    if (decimal.TryParse(parts[0], out decimal min) && decimal.TryParse(parts[1], out decimal max))
                    {
                         query = query.Where(p => p.Variantes.Any(v => v.prix >= min && v.prix <= max));
                    }
                }
                else if (priceRange.EndsWith("+"))
                {
                     if (decimal.TryParse(priceRange.TrimEnd('+'), out decimal min))
                     {
                         query = query.Where(p => p.Variantes.Any(v => v.prix >= min));
                     }
                }
            }

            if (inStock == true)
            {
                query = query.Where(p => p.Variantes.Any(v => v.quantite > 0));
            }

            if (isNew == true)
            {
                var thresholdDate = DateTime.Now.AddDays(-30);
                query = query.Where(p => p.created_at >= thresholdDate);
            }

            var products = await query
                .Select(p => new ProductViewModel
                {
                    Id = p.idP,
                    Name = p.nomP,
                    Category = p.Categorie.nom,
                    Cooperative = p.Admin.nom_cooperative,
                    Price = p.Variantes.OrderBy(v => v.prix).Select(v => v.prix).FirstOrDefault(),
                    Image = p.Variantes.OrderBy(v => v.idV).Select(v => v.photo).FirstOrDefault() ?? "https://picsum.photos/seed/placeholder/300/300.jpg",
                    Rating = p.Avis.Where(a => a.note.HasValue).Average(a => (double?)a.note) ?? 0,
                    Reviews = p.Avis.Count(a => a.note.HasValue),
                    IsBestSeller = false, // Logic for bestseller can be added later
                    IsNew = p.created_at >= DateTime.Now.AddDays(-30)
                })
                .OrderByDescending(p => p.IsNew)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return View("~/Views/Product/Catalog.cshtml", products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Produits
                .AsNoTracking()
                .Include(p => p.Variantes)
                .Include(p => p.Categorie)
                .Include(p => p.Admin)
                .Include(p => p.Avis)
                .FirstOrDefaultAsync(p => p.idP == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                Id = product.idP,
                Name = product.nomP,
                Description = product.description ?? string.Empty,
                Category = product.Categorie.nom,
                Cooperative = product.Admin.nom_cooperative,
                Price = product.Variantes.OrderBy(v => v.prix).Select(v => v.prix).FirstOrDefault(),
                Rating = product.Avis.Where(a => a.note.HasValue).Average(a => (double?)a.note) ?? 0,
                Reviews = product.Avis.Count(a => a.note.HasValue),
                Images = product.Variantes
                            .Where(v => !string.IsNullOrEmpty(v.photo))
                            .Select(v => v.photo!)
                            .Distinct()
                            .ToList(),
                Variants = product.Variantes.Select(v => new VarianteViewModel
                {
                    Id = v.idV,
                    Price = v.prix,
                    Size = v.taille ?? string.Empty,
                    Color = v.couleur ?? string.Empty,
                    Stock = v.quantite,
                    Photo = v.photo
                }).ToList(),
                IsNew = product.created_at >= DateTime.Now.AddDays(-30)
            };

            // Enhance images if none found (fallback)
            if (!viewModel.Images.Any())
            {
                viewModel.Images.Add("https://picsum.photos/seed/placeholder/800/800.jpg");
            }
            
            // Populate legacy Image property for compatibility if needed
            viewModel.Image = viewModel.Images.First();

            return View("~/Views/Product/Detail.cshtml", viewModel);
        }
    }
}
