using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class LigneVente
    {
        [Key]
        public int Id { get; set; }

        public int EtatDesVentesId { get; set; }
        [ForeignKey("EtatDesVentesId")]
        public EtatDesVentes? EtatDesVentes { get; set; }

        public int ArticleId { get; set; }
        [ForeignKey("ArticleId")]
        public Article? Article { get; set; }

        [Display(Name = "Dotation (Qte initiale)")]
        public int QuantiteDotation { get; set; } = 0;

        [Display(Name = "Complément")]
        public int QuantiteCompl { get; set; } = 0;

        [Display(Name = "Vendue")]
        public int QuantiteVendue { get; set; } = 0;

        [Display(Name = "Prix Unitaire (EUR)")]
        public decimal PrixUnitaireEUR { get; set; } = 0;

        [Display(Name = "Valeur (EUR)")]
        public decimal ValeurTotaleEUR => QuantiteVendue * PrixUnitaireEUR;
    }
}
