using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet__E_commerce.Models
{
    public class SuperAdmin
    {
        [Key]
        [ForeignKey(nameof(Utilisateur))]
        public int id { get; set; }

        // Navigation property
        public virtual Utilisateur Utilisateur { get; set; } = null!;
    }
}
