using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Categorie
    {
        [Key]
        public int idC { get; set; }

        [Required]
        [StringLength(50)]
        public string nom { get; set; } = string.Empty;

        public string? description { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
    }
}
