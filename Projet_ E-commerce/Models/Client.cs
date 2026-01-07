using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Client
    {
        [Key]
        [ForeignKey(nameof(Utilisateur))]
        public int id { get; set; }

        [Required]
        [StringLength(255)]
        public string prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string nom { get; set; } = string.Empty;

        [StringLength(100)]
        public string? telephone { get; set; }

        public DateTime? date_naissance { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Utilisateur Utilisateur { get; set; } = null!;

        // Navigation properties for related entities
        public virtual ICollection<AdresseLivraison> AdressesLivraison { get; set; } = new List<AdresseLivraison>();
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
        public virtual ICollection<Avis> Avis { get; set; } = new List<Avis>();
    }
}
