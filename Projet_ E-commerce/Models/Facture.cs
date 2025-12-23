using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Facture
    {
        [Key]
        public int idFacture { get; set; }

        [ForeignKey(nameof(Commande))]
        public int idCommande { get; set; }

        [Required]
        [StringLength(50)]
        public string numero_facture { get; set; } = string.Empty;

        [StringLength(255)]
        public string? path_facture { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Commande Commande { get; set; } = null!;
    }
}
