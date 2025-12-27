namespace Projet__E_commerce.Models.ViewModels
{
    public class CooperativeDetailsViewModel
    {
        public int Id { get; set; }
        public string NomCooperative { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Ville { get; set; }
        public string? Localisation { get; set; }
        public string? Telephone { get; set; }
        public string? Logo { get; set; }
        public string? Description { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Statistiques
        public int TotalProduits { get; set; }
        public int TotalCommandes { get; set; }
        public decimal Revenue { get; set; }
        
        // Produits
        public List<ProductViewModel> Produits { get; set; } = new();
    }

    public class ProductViewModel
    {
        public int IdP { get; set; }
        public string NomP { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public double? MoyenneAvis { get; set; }
        public int NombreAvis { get; set; }
        public List<AvisViewModel> Avis { get; set; } = new();
    }

    public class AvisViewModel
    {
        public int IdAvis { get; set; }
        public string ClientNom { get; set; } = string.Empty;
        public string ClientPrenom { get; set; } = string.Empty;
        public int Note { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateAvis { get; set; }
    }
}
