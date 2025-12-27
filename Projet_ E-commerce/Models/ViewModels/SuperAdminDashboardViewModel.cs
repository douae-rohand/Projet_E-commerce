namespace Projet__E_commerce.Models.ViewModels
{
    public class SuperAdminDashboardViewModel
    {
        public int TotalCooperatives { get; set; }
        public int TotalCooperativesActives { get; set; }
        public int TotalUsers { get; set; }
        public int TotalUsersActifs { get; set; }
        public int TotalClients { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProduits { get; set; }
        public int ProduitsEnStock { get; set; }
        public int ProduitsStockFaible { get; set; }
        public int ProduitsRupture { get; set; }
        public List<CooperativeStatsViewModel> TopCooperatives { get; set; } = new();
        public List<ProductPerformanceViewModel> TopProduits { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class CooperativeStatsViewModel
    {
        public int Id { get; set; }
        public string NomCooperative { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int TotalProduits { get; set; }
        public int TotalCommandes { get; set; }
        public decimal Revenue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserStatsViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? NomCooperative { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecentActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class AnalyticsViewModel
    {
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();
        public List<CategorySalesViewModel> CategorySales { get; set; } = new();
        public List<RegionStatsViewModel> RegionStats { get; set; } = new();
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class CategorySalesViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalSales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RegionStatsViewModel
    {
        public string Region { get; set; } = string.Empty;
        public int Cooperatives { get; set; }
        public int Products { get; set; }
        public decimal Revenue { get; set; }
    }

    public class AlertViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProductPerformanceViewModel
    {
        public int IdProduit { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public string NomCooperative { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Prix { get; set; }
        public int NombreVentes { get; set; }
        public decimal RevenusGeneres { get; set; }
        public double? MoyenneAvis { get; set; }
    }
}
