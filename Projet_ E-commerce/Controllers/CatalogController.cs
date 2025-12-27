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

        public async Task<IActionResult> Index(int? category)
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
                .AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(p => p.idC == category.Value);
            }

            var products = await query
                .Select(p => new ProductViewModel
                {
                    Id = p.idP,
                    Name = p.nomP,
                    Category = p.Categorie.nom,
                    Cooperative = p.Admin.nom_cooperative,
                    Price = p.Variantes
                        .OrderBy(v => v.prix)
                        .Select(v => v.prix)
                        .FirstOrDefault(),
                    Image = p.Variantes
                        .OrderBy(v => v.idV)
                        .Select(v => v.photo)
                        .FirstOrDefault() != null ? 
                        $"https://picsum.photos/seed/{p.Variantes.OrderBy(v => v.idV).Select(v => v.photo).FirstOrDefault()}/300/300.jpg" : 
                        "https://picsum.photos/seed/placeholder/300/300.jpg",
                    Rating = p.Avis
                        .Where(a => a.note.HasValue)
                        .Average(a => (double?)a.note) ?? 0,
                    Reviews = p.Avis.Count(a => a.note.HasValue),
                    IsBestSeller = false,
                    IsNew = p.created_at >= DateTime.Now.AddDays(-30)
                })
                .OrderByDescending(p => p.IsNew)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return View("~/Views/Product/Catalog.cshtml", products);
        }
    }
}
