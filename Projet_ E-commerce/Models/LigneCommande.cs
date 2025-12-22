using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class LigneCommande
    {
        [Key]
        public int idLC { get; set; }

        [ForeignKey(nameof(Commande))]
        public int idCommande { get; set; }

        [ForeignKey(nameof(Variante))]
        public int idV { get; set; }

        public int quantite { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal prix_unitaire { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Commande Commande { get; set; } = null!;
        public virtual Variante Variante { get; set; } = null!;
    }
}
