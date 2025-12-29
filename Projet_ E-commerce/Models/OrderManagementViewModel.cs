using System.ComponentModel.DataAnnotations;

namespace Projet__E_commerce.Models
{
    public class OrderManagementViewModel
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
        // Total des lignes appartenant à la coop de l'admin (pas toute la commande globale)
        public decimal PrixTotalAdmin { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Informations de livraison
        public string? ModeLivraison { get; set; }
        public DateTime? DateDebutEstimation { get; set; }
        public DateTime? DateFinEstimation { get; set; }
        
        // Lignes de commande (pour les détails)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<LigneCommandeDto> LignesCommande { get; set; } = new();
    }

    public class LigneCommandeDto
    {
        public int IdLC { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public string? Taille { get; set; }
        public string? Couleur { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public string? Photo { get; set; }
        public decimal SousTotal => Quantite * PrixUnitaire;
    }
}
