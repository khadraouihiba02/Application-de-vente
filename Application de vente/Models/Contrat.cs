using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class Contrat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nom du Contrat (ex: Été 2026)")]
        public string NomContrat { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }

        public string Description { get; set; }

        // Navigation property
        public ICollection<PrixArticleContrat> PrixArticles { get; set; }
    }
}
