using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Projet__E_commerce.Models
{
    public class Variante
    {
        [Key]
        public int idV { get; set; }

        [ForeignKey(nameof(Produit))]
        public int idP { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal prix { get; set; }

        [StringLength(20)]
        public string? taille { get; set; }

        [StringLength(20)]
        public string? couleur { get; set; }

        [StringLength(255)]
        public string? photo { get; set; }

        public int quantite { get; set; } = 0;

        [StringLength(50)]
        public string? poids { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Produit? Produit { get; set; } = null!;
        public virtual ICollection<LigneCommande> LignesCommande { get; set; } = new List<LigneCommande>();

        [NotMapped]
        public IFormFile? PhotoFile { get; set; }
    }
}
