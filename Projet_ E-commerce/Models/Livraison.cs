using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Livraison
    {
        [Key]
        public int idLivraison { get; set; }

        [ForeignKey(nameof(Commande))]
        public int idCommande { get; set; }

        [ForeignKey(nameof(AdresseLivraison))]
        public int idAdresse { get; set; }

        [Required]
        public DateTime dateDebutEstimation { get; set; }

        [Required]
        public DateTime dateFinEstimation { get; set; }

        public string? notes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal frais { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string mode_livraison { get; set; } = "Standard";

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Commande Commande { get; set; } = null!;
        public virtual AdresseLivraison AdresseLivraison { get; set; } = null!;
        public virtual BordereauLivraison BordereauLivraison { get; set; } = null!;
    }
}
