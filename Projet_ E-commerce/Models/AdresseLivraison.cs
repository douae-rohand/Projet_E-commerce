using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class AdresseLivraison
    {
        [Key]
        public int idAdresse { get; set; }

        [ForeignKey(nameof(Client))]
        public int idClient { get; set; }

        [StringLength(100)]
        public string? nom_adresse { get; set; } // Ex: "Maison", "Bureau"

        [Required]
        [StringLength(255)]
        public string adresse_complete { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ville { get; set; } = string.Empty;

        [StringLength(20)]
        public string? code_postal { get; set; }

        [StringLength(100)]
        public string? telephone { get; set; }

        public bool est_par_defaut { get; set; } = false;

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();
    }
}
