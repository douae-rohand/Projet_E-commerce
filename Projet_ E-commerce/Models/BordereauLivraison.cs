using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class BordereauLivraison
    {
        [Key]
        public int idBordereau { get; set; }

        [ForeignKey(nameof(Livraison))]
        public int idLivraison { get; set; }

        [Required]
        [StringLength(50)]
        public string numero_bordereau { get; set; } = string.Empty;

        public DateTime date_generation { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? path_bordereau { get; set; }

        // Navigation properties
        public virtual Livraison Livraison { get; set; } = null!;
    }
}
