using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCooperativesAsync()
        {
            return await _context.Admins.CountAsync();
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _context.Produits
                .Where(p => p.statut == "active")
                .CountAsync();
        }

        public async Task<int> GetTotalClientsAsync()
        {
            return await _context.Clients.CountAsync();
        }
        
        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Commandes.CountAsync();
        }

        public async Task<Models.DashboardStats> GetDashboardStatsAsync()
        {
            var cooperativesCount = await GetTotalCooperativesAsync();
            var productsCount = await GetTotalProductsAsync();
            var clientsCount = await GetTotalClientsAsync();
            var ordersCount = await GetTotalOrdersAsync();
            var categories = await _context.Categories
                .Include(c => c.Produits.Where(p => p.statut == "active"))
                .ToListAsync();

            var reviews = await _context.Avis
                .Include(a => a.Client)
                .Include(a => a.Produit)
                .OrderByDescending(a => a.created_at)
                .Take(4)
                .ToListAsync();

            var cooperatives = await GetAllCooperativesAsync();

            // Fetch Best Sellers (Products with most line items)
            var bestSellerIds = await _context.LignesCommande
                .GroupBy(lc => lc.Variante.idP)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(4)
                .ToListAsync();

            var bestSellers = await _context.Produits
                .Where(p => bestSellerIds.Contains(p.idP) && p.statut == "active")
                .Include(p => p.Categorie)
                .Include(p => p.Admin)
                .Include(p => p.Variantes)
                .Include(p => p.Avis)
                .AsNoTracking() // Optimize performance
                .ToListAsync();

            // Fetch Newest Products (excluding best sellers to avoid duplicates if possible, though distinct handles it)
            var recentProducts = await _context.Produits
                .Where(p => p.statut == "active" && !bestSellerIds.Contains(p.idP))
                .OrderByDescending(p => p.created_at)
                .Take(4)
                .Include(p => p.Categorie)
                .Include(p => p.Admin)
                .Include(p => p.Variantes)
                .Include(p => p.Avis)
                .AsNoTracking()
                .ToListAsync();

            var featuredRaw = bestSellers.Concat(recentProducts).ToList();

            var featuredViewModels = featuredRaw.Select(p => new ProductViewModel
            {
                Id = p.idP,
                Name = p.nomP,
                Category = p.Categorie.nom,
                Cooperative = p.Admin.nom_cooperative,
                Price = p.Variantes.OrderBy(v => v.prix).Select(v => v.prix).FirstOrDefault(),
                Image = p.Variantes.OrderBy(v => v.idV).Select(v => v.photo).FirstOrDefault() != null ? 
                        $"https://picsum.photos/seed/{p.Variantes.OrderBy(v => v.idV).Select(v => v.photo).FirstOrDefault()}/300/300.jpg" : 
                        "https://picsum.photos/seed/placeholder/300/300.jpg",
                Rating = p.Avis.Where(a => a.note.HasValue).Average(a => (double?)a.note) ?? 0,
                Reviews = p.Avis.Count(a => a.note.HasValue),
                IsBestSeller = bestSellerIds.Contains(p.idP),
                IsNew = p.created_at >= DateTime.Now.AddDays(-30)
            }).ToList();

            return new DashboardStats
            {
                TotalCooperatives = cooperativesCount,
                TotalProducts = productsCount,
                TotalClients = clientsCount,
                TotalOrders = ordersCount,
                Categories = categories,
                RecentReviews = reviews,
                Cooperatives = cooperatives,
                FeaturedProducts = featuredViewModels
            };
        }
        public async Task<List<Avis>> GetTopReviewsAsync()
        {
            return await _context.Avis
                .Include(a => a.Client)
                .Include(a => a.Produit)
                .Where(a => a.note >= 4)
                .OrderByDescending(a => a.created_at)
                .ToListAsync();
        }

        public async Task<List<Admin>> GetAllCooperativesAsync()
        {
            return await _context.Admins
                .Include(a => a.Produits)
                    .ThenInclude(p => p.Categorie)
                .Include(a => a.Produits)
                    .ThenInclude(p => p.Avis)
                .ToListAsync();
        }
    }
}
