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

        public async Task<Models.DashboardStats> GetDashboardStatsAsync()
        {
            var cooperativesCount = await GetTotalCooperativesAsync();
            var productsCount = await GetTotalProductsAsync();
            var clientsCount = await GetTotalClientsAsync();
            var categories = await _context.Categories
                .Take(4)
                .ToListAsync();

            return new DashboardStats
            {
                TotalCooperatives = cooperativesCount,
                TotalProducts = productsCount,
                TotalClients = clientsCount,
                Categories = categories
            };
        }
    }
}
