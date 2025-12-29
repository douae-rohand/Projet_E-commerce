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
        public List<OrderDetailsViewModel> RecentOrders { get; set; } = new();
        // NEW: bind user email from DB/Identity rather than ViewBag
        // Added to satisfy _DashboardOverview.cshtml bindings
        // Added: used by _DashboardOverview.cshtml header
        public string? UserEmail { get; set; }

        // Added: used by stock doughnut in _DashboardOverview.cshtml
        public int ProduitsStockOk { get; set; }
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
    public class OrderDetailsViewModel
    {
        public int IdCommande { get; set; }
        public DateTime DateCommande { get; set; }
        public string Statut { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public string ClientNom { get; set; } = string.Empty;
        public string ClientPrenom { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientTelephone { get; set; } = string.Empty;
        public string AdresseLivraison { get; set; } = string.Empty;
        public string VilleLivraison { get; set; } = string.Empty;
        public string CodePostalLivraison { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new();
        public string? Notes { get; set; }

        // Properties for view compatibility
        public string AdresseComplete => $"{AdresseLivraison}, {CodePostalLivraison} {VilleLivraison}";
        public string VilleAdresse => VilleLivraison;
        public decimal PrixTotal => MontantTotal;
        public decimal SousTotal => MontantTotal; // Simplified
        public DateTime CreatedAt => DateCommande;
        public List<OrderItemViewModel> OrderItems => Items;
        public bool HasDelivery { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? DeliveryMode { get; set; }
        public decimal FraisLivraison { get; set; }

        // Facture and Bordereau
        public string? NumeroFacture { get; set; }
        public string? PathFacture { get; set; }
        public string? NumeroBordereau { get; set; }
        public string? PathBordereau { get; set; }
    }

    public class OrderItemViewModel
    {
        public int IdProduit { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public string NomCooperative { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal SousTotal { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Taille { get; set; }
        public string? Couleur { get; set; }
        public string? Poids { get; set; }
    }
}
