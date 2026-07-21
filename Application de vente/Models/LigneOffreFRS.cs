using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class LigneOffreFRS
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EtatDesOffresFRSId { get; set; }

        [ForeignKey("EtatDesOffresFRSId")]
        public EtatDesOffresFRS? EtatDesOffresFRS { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeArticle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NomArticle { get; set; } = string.Empty;

        [Display(Name = "Dotation Initiale FRS")]
        public int DotationInitialeFRS { get; set; } = 0;

        [Display(Name = "Quantité Restante FRS")]
        public int QuantiteRestanteFRS { get; set; } = 0;

        [Display(Name = "Quantité Consommée FRS")]
        public int QuantiteConsommeeFRS { get; set; } = 0;
    }
}
