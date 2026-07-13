using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class PrixArticleContrat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Article")]
        public int ArticleId { get; set; }
        [ForeignKey("ArticleId")]
        public Article Article { get; set; }

        [Required]
        [Display(Name = "Contrat")]
        public int ContratId { get; set; }
        [ForeignKey("ContratId")]
        public Contrat Contrat { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Prix Unitaire (EUR)")]
        public decimal PrixUnitaire { get; set; }
    }
}
