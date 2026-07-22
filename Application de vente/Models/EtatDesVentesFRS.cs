using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesVentesFRS
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro d'état FRS est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "N° État FRS")]
        public string NumeroEtat { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de réception est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Réception")]
        public DateTime DateReception { get; set; } = DateTime.Today;

        [Display(Name = "Montant FRS (EUR)")]
        public decimal MontantFRS { get; set; } = 0;

        public decimal TauxChangeApplique { get; set; } = 1;
        public decimal ChiffreAffairesEUR { get; set; } = 0;
        public decimal MontantTheoriqueTND { get; set; } = 0;
        public decimal MontantDeclareReelTND { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Statut de Contrôle")]
        public string StatutControle { get; set; } = "En attente"; // Ex: "En attente", "Conforme", "Écart"

        [Required(ErrorMessage = "Veuillez sélectionner l'état des ventes PNC associé.")]
        [Display(Name = "État des Ventes PNC Associé")]
        public int EtatDesVentesId { get; set; }

        [ForeignKey("EtatDesVentesId")]
        public EtatDesVentes? EtatDesVentes { get; set; }

        // Relation One-to-Many avec les lignes de vente FRS
        public ICollection<LigneVenteFRS> Lignes { get; set; } = new List<LigneVenteFRS>();
    }
}
