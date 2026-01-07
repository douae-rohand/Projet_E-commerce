using System.Collections.Generic;

namespace Projet__E_commerce.Models
{
    public class DashboardStats
    {
        public int TotalCooperatives { get; set; }
        public int TotalProducts { get; set; }
        public int TotalClients { get; set; }
        public int TotalOrders { get; set; }
        public List<Admin> Cooperatives { get; set; } = new();
        public List<Categorie> Categories { get; set; } = new();
        public List<Avis> RecentReviews { get; set; } = new();
        public List<ProductViewModel> FeaturedProducts { get; set; } = new();
    }
}
