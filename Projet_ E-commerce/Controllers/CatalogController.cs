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

        public async Task<IActionResult> Index(int? category, bool? inStock, bool? isPromo, bool? isNew, int? cooperative, string search, string? sort)
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.nom)
                .ToListAsync();

            var cooperatives = await _db.Admins
                .AsNoTracking()
                .OrderBy(a => a.nom_cooperative)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.Cooperatives = cooperatives;
            ViewBag.SelectedCooperative = cooperative;
            ViewBag.SearchTerm = search;
            ViewBag.Sort = sort;

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

            if (cooperative.HasValue)
            {
                query = query.Where(p => p.idAdmin == cooperative.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.nomP.ToLower().Contains(search) || 
                                         p.Categorie.nom.ToLower().Contains(search) || 
                                         p.Admin.nom_cooperative.ToLower().Contains(search));
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

            var productQuery = query.Select(p => new ProductViewModel
            {
                Id = p.idP,
                Name = p.nomP,
                Category = p.Categorie.nom ?? "Divers",
                Cooperative = p.Admin != null ? p.Admin.nom_cooperative ?? "Coopérative" : "Coopérative",
                Price = p.Variantes.OrderBy(v => v.prix).Select(v => v.prix).FirstOrDefault(),
                Image = p.Variantes.OrderBy(v => v.idV).Select(v => v.photo).FirstOrDefault() ?? "https://picsum.photos/seed/placeholder/300/300.jpg",
                Rating = p.Avis.Where(a => a.note.HasValue).Average(a => (double?)a.note) ?? 0,
                Reviews = p.Avis.Count(a => a.note.HasValue),
                IsBestSeller = false,
                IsNew = p.created_at >= DateTime.Now.AddDays(-30),
                CreatedAt = p.created_at
            });

            // Apply Sorting
            productQuery = sort switch
            {
                "price-asc" => productQuery.OrderBy(p => p.Price),
                "price-desc" => productQuery.OrderByDescending(p => p.Price),
                "newest" => productQuery.OrderByDescending(p => p.CreatedAt),
                "popular" => productQuery.OrderByDescending(p => p.Reviews),
                _ => productQuery.OrderByDescending(p => p.IsNew).ThenBy(p => p.Name)
            };

            var products = await productQuery.ToListAsync();

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
                Category = product.Categorie.nom ?? "Divers",
                Cooperative = product.Admin != null ? product.Admin.nom_cooperative ?? "Coopérative" : "Coopérative",
                Price = product.Variantes.OrderBy(v => v.prix).Select(v => v.prix).FirstOrDefault(),
                Rating = product.Avis != null && product.Avis.Any(a => a.note.HasValue) 
                           ? product.Avis.Where(a => a.note.HasValue).Average(a => (double?)a.note) ?? 0 
                           : 0,
                Reviews = product.Avis?.Count(a => a.note.HasValue) ?? 0,
                Images = product.Variantes
                            .Where(v => !string.IsNullOrEmpty(v.photo))
                            .Select(v => v.photo ?? "https://picsum.photos/seed/placeholder/800/800.jpg")
                            .Distinct()
                            .ToList(),
                Variants = product.Variantes.Select(v => new VarianteViewModel
                {
                    Id = v.idV,
                    Price = v.prix,
                    Size = v.taille ?? string.Empty,
                    Color = v.couleur ?? string.Empty,
                    Weight = v.poids ?? string.Empty,
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
