using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class Utilisateur
    {
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        public bool est_actif { get; set; } = true;

        [Required]
        [StringLength(255)]
        public string password { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;
    }
}
