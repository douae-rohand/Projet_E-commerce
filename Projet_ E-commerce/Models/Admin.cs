using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Admin
    {
        [Key]
        [ForeignKey(nameof(Utilisateur))]
        public int id { get; set; }

        [Required]
        [StringLength(255)]
        public string nom_cooperative { get; set; } = string.Empty;

        public string? localisation { get; set; }

        [StringLength(255)]
        public string? ville { get; set; }

        [StringLength(255)]
        public string? logo { get; set; }

        [StringLength(100)]
        public string? telephone { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Utilisateur Utilisateur { get; set; } = null!;

        // Navigation properties for related entities
        public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
    }
}
