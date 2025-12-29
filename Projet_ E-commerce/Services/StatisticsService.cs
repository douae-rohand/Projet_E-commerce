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

            return new DashboardStats
            {
                TotalCooperatives = cooperativesCount,
                TotalProducts = productsCount,
                TotalClients = clientsCount,
                TotalOrders = ordersCount,
                Categories = categories,
                RecentReviews = reviews,
                Cooperatives = cooperatives
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
