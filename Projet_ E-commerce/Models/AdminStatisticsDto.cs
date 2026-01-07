namespace Projet__E_commerce.Models
{
    // DTO pour les statistiques retournées par sp_get_admin_statistics
    public class AdminStatisticsDto
    {
        public decimal VentesMois { get; set; }
        public decimal VentesPourcentage { get; set; }
        public int CommandesMois { get; set; }
        public decimal CommandesPourcentage { get; set; }
        public int ProduitsActifs { get; set; }
    }
}
