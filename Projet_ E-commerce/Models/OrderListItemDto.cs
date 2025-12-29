namespace Projet__E_commerce.Models
{
    // DTO simple pour la liste des commandes sans propriétés de navigation
    // Cela évite l'erreur "Navigations are not supported when using SqlQuery"
    public class OrderListItemDto
    {
        public int IdCommande { get; set; }
        public string NumeroCommande { get; set; } = string.Empty;
        public int IdClient { get; set; }
        public string NomClient { get; set; } = string.Empty;
        public string PrenomClient { get; set; } = string.Empty;
        public string? TelephoneClient { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public decimal PrixTotal { get; set; }
        public decimal PrixTotalAdmin { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Informations de livraison
        public string? StatutLivraison { get; set; }
        public string? ModeLivraison { get; set; }
        public DateTime? DateDebutEstimation { get; set; }
        public DateTime? DateFinEstimation { get; set; }
    }
}
