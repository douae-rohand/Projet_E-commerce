using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Avis
    {
        [Key]
        public int idAvis { get; set; }

        [ForeignKey(nameof(Client))]
        public int idClient { get; set; }

        [ForeignKey(nameof(Produit))]
        public int idProduit { get; set; }

        [Range(1, 5)]
        public int? note { get; set; }

        public string? commentaire { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual Produit Produit { get; set; } = null!;
    }
}
