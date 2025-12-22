using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Commande
    {
        [Key]
        public int idCommande { get; set; }

        [ForeignKey(nameof(Client))]
        public int idClient { get; set; }

        [Required]
        [StringLength(20)]
        public string statut { get; set; } = "en_attente";

        [Column(TypeName = "decimal(10,2)")]
        public decimal prixTotal { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();
        public virtual Livraison Livraison { get; set; } = null!;
        public virtual Facture Facture { get; set; } = null!;
    }
}
