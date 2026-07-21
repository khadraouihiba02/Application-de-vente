using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class LigneVenteFRS
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EtatDesVentesFRSId { get; set; }

        [ForeignKey("EtatDesVentesFRSId")]
        public EtatDesVentesFRS? EtatDesVentesFRS { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeArticle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NomArticle { get; set; } = string.Empty;

        [Display(Name = "Quantité Vendue FRS")]
        public int QuantiteVendueFRS { get; set; } = 0;

        [Display(Name = "Prix Unitaire FRS (EUR)")]
        public decimal PrixUnitaireFRS { get; set; } = 0;

        [Display(Name = "Valeur FRS (EUR)")]
        public decimal ValeurFRS { get; set; } = 0;
    }
}
