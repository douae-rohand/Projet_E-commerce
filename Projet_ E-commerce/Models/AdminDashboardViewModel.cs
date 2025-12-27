using System.ComponentModel.DataAnnotations;

namespace Projet__E_commerce.Models
{
    public class AdminDashboardViewModel
    {
        public decimal VentesMois { get; set; }
        public decimal VentesPourcentage { get; set; }
        public int CommandesMois { get; set; }
        public decimal CommandesPourcentage { get; set; }
        public int ProduitsActifs { get; set; }
        
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        
        public string CooperativeName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class RecentOrderDto
    {
        public int IdCommande { get; set; }
        public string NumeroCommande { get; set; } = string.Empty;
        public string NomClient { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public decimal PrixTotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TopProductDto
    {
        public int IdP { get; set; }
        public string NomP { get; set; } = string.Empty;
        public int NombreVentes { get; set; }
        public decimal RevenuTotal { get; set; }
    }
}
