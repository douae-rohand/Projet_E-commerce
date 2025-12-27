using System.ComponentModel.DataAnnotations;

namespace Projet__E_commerce.Models
{
    public class AdminProfileViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le nom de la coopérative est requis")]
        [StringLength(255)]
        [Display(Name = "Nom de la coopérative")]
        public string NomCooperative { get; set; } = string.Empty;
        
        [Display(Name = "Localisation")]
        public string? Localisation { get; set; }
        
        [StringLength(255)]
        [Display(Name = "Ville")]
        public string? Ville { get; set; }
        
        [StringLength(100)]
        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Logo")]
        public string? Logo { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool EstActif { get; set; }
    }
}
