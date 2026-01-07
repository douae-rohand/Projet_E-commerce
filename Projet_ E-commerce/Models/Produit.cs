using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Produit
    {
        [Key]
        public int idP { get; set; }

        [Required]
        [StringLength(255)]
        public string nomP { get; set; } = string.Empty;

        public string? description { get; set; }

        public int seuil_alerte { get; set; } = 10;

        [Required]
        [StringLength(20)]
        public string statut { get; set; } = "active";

        [ForeignKey(nameof(Categorie))]
        public int idC { get; set; }

        [ForeignKey(nameof(Admin))]
        public int idAdmin { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Categorie Categorie { get; set; } = null!;
        public virtual Admin Admin { get; set; } = null!;
        public virtual ICollection<Variante> Variantes { get; set; } = new List<Variante>();
        public virtual ICollection<Avis> Avis { get; set; } = new List<Avis>();
    }
}
