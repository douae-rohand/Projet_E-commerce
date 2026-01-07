using Projet__E_commerce.Models;

namespace Projet__E_commerce.Services
{
    public interface IStatisticsService
    {
        Task<int> GetTotalCooperativesAsync();
        Task<int> GetTotalProductsAsync();
        Task<int> GetTotalClientsAsync();
        Task<int> GetTotalOrdersAsync();
        Task<Models.DashboardStats> GetDashboardStatsAsync();
        Task<List<Avis>> GetTopReviewsAsync();
        Task<List<Admin>> GetAllCooperativesAsync();
    }
}
