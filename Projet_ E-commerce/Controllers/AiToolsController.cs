using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiToolsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AiToolsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? query = null)
        {
            var productsQuery = _db.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Variantes)
                .Where(p => p.statut == "active" || p.statut == "DISPONIBLE");

            // If a query is provided, we ONLY return matches for that query.
            if (!string.IsNullOrWhiteSpace(query))
            {
                var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                // Build a combined 'Like' search for better matching
                productsQuery = productsQuery.Where(p => 
                    keywords.Any(k => 
                        EF.Functions.Like(p.nomP, $"%{k}%") || 
                        (p.description != null && EF.Functions.Like(p.description, $"%{k}%")) || 
                        p.Categorie.nom.Contains(k)));
            }
            else 
            {
                // Fallback to top 5 ONLY if the query is truly empty
                productsQuery = productsQuery.OrderByDescending(p => p.idP).Take(5);
            }

            var results = await productsQuery
                .Select(p => new
                {
                    p.idP,
                    p.nomP,
                    Category = p.Categorie.nom,
                    MinPrice = p.Variantes.Any() ? p.Variantes.Min(v => v.prix) : 0,
                    InStock = p.Variantes.Any() && p.Variantes.Sum(v => v.quantite) > 0,
                    p.description
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("list_debug")]
        public async Task<IActionResult> ListDebug()
        {
            var results = await _db.Produits
                .Include(p => p.Categorie)
                .Select(p => new { p.idP, p.nomP, p.statut, Category = p.Categorie.nom })
                .Take(5)
                .ToListAsync();
            return Ok(results);
        }
        public IActionResult Ping()
        {
            return Ok(new { status = "healthy", time = DateTime.Now });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.Categories
                .Select(c => new { c.idC, c.nom })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpGet("details/{id?}")]
        public async Task<IActionResult> GetProductDetails(string? id)
        {
            // 1. Try path variable
            string? rawId = id;

            // 2. Try common query parameters
            if (string.IsNullOrWhiteSpace(rawId)) rawId = Request.Query["id"].ToString();
            if (string.IsNullOrWhiteSpace(rawId)) rawId = Request.Query["idP"].ToString();
            if (string.IsNullOrWhiteSpace(rawId)) rawId = Request.Query["productId"].ToString();
            if (string.IsNullOrWhiteSpace(rawId)) rawId = Request.Query["id_p"].ToString();
            if (string.IsNullOrWhiteSpace(rawId)) rawId = Request.Query["product_id"].ToString();

            // 3. ULTIMATE FALLBACK: If AI guessed a weird name, just take the first query param value
            if (string.IsNullOrWhiteSpace(rawId))
            {
                var firstParam = Request.Query.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstParam.Key))
                {
                    rawId = firstParam.Value.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(rawId))
                return BadRequest("Missing Product ID. Please ensure your tool is sending a parameter.");

            // Fuzzy parsing: extract number from "ID: 12", "Id 12", etc.
            var match = System.Text.RegularExpressions.Regex.Match(rawId, @"\d+");
            if (!match.Success || !int.TryParse(match.Value, out int productId))
                return BadRequest($"Invalid ID format: {rawId}");

            var product = await _db.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Variantes)
                .Include(p => p.Admin)
                .FirstOrDefaultAsync(p => p.idP == productId);

            if (product == null)
                return NotFound($"Product #{productId} not found.");

            return Ok(new
            {
                product.idP,
                product.nomP,
                product.description,
                Category = product.Categorie?.nom ?? "N/A",
                Cooperative = product.Admin?.nom_cooperative ?? "Authentique Maroc",
                Variants = product.Variantes?.Select(v => new
                {
                    v.idV,
                    v.taille,
                    v.couleur,
                    v.prix,
                    v.quantite
                })
            });
        }
    }
}
