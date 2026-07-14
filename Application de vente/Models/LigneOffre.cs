using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class LigneOffre
    {
        [Key]
        public int Id { get; set; }

        public int EtatDesOffresId { get; set; }
        [ForeignKey("EtatDesOffresId")]
        public EtatDesOffres? EtatDesOffres { get; set; }

        public int ArticleId { get; set; }
        [ForeignKey("ArticleId")]
        public Article? Article { get; set; }

        [Display(Name = "Dotation")]
        public int QuantiteDotation { get; set; } = 0;

        [Display(Name = "Complément")]
        public int QuantiteCompl { get; set; } = 0;

        [Display(Name = "Quantité Offerte")]
        public int QuantiteOfferte { get; set; } = 0;

        [Display(Name = "Prix Promo (EUR)")]
        public decimal PrixUnitairePromoEUR { get; set; } = 0;

        // Propriété calculée : valeur totale de l'offre
        [NotMapped]
        public decimal ValeurTotaleEUR => QuantiteOfferte * PrixUnitairePromoEUR;
    }
}
