using System.ComponentModel.DataAnnotations;

namespace Projet__E_commerce.Models
{
    public class ProductManagementViewModel
    {
        public int IdP { get; set; }
        
        [Required(ErrorMessage = "Le nom du produit est requis")]
        [StringLength(255)]
        public string NomP { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Le seuil d'alerte doit être entre 0 et 1000")]
        public int SeuilAlerte { get; set; } = 10;
        
        [Required(ErrorMessage = "La catégorie est requise")]
        public int IdC { get; set; }
        
        public string Statut { get; set; } = "active";
        
        // Première variante
        [Required(ErrorMessage = "Le prix est requis")]
        [Range(0.01, 999999.99, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal Prix { get; set; }
        
        public string? Taille { get; set; }
        
        public string? Couleur { get; set; }
        
        public string? Photo { get; set; }
        
        [Required(ErrorMessage = "La quantité est requise")]
        [Range(0, 999999, ErrorMessage = "La quantité doit être positive")]
        public int Quantite { get; set; }
        
        public string? Poids { get; set; }
        
        // Pour l'affichage
        public string? NomCategorie { get; set; }
        public int NombreVariantes { get; set; }
        public int StockTotal { get; set; }
        public decimal? PrixMin { get; set; }
        public decimal? PrixMax { get; set; }
        
        // Liste des catégories pour le dropdown
        public List<Categorie> Categories { get; set; } = new();
        
        // Liste des variantes existantes
        public List<Variante> Variantes { get; set; } = new();
    }

    public class ProductListItemViewModel
    {
        public int IdP { get; set; }
        public string NomP { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SeuilAlerte { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string NomCategorie { get; set; } = string.Empty;
        public int NombreVariantes { get; set; }
        public int StockTotal { get; set; }
        public decimal? PrixMin { get; set; }
        public decimal? PrixMax { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
