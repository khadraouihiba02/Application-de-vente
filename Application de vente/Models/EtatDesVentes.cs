using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesVentes
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro de Feuille de Ligne est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "N° Feuille de Ligne (FL)")]
        public string NumeroFeuilleLigne { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date du Vol")]
        public DateTime DateVol { get; set; } = DateTime.Today;

        [Display(Name = "Vols Associés")]
        public ICollection<EtatDesVentesVol> VolsList { get; set; } = new List<EtatDesVentesVol>();

        [Required(ErrorMessage = "Veuillez sélectionner le PNC vendeur.")]
        [Display(Name = "PNC Vendeur")]
        public int PNCVendeurId { get; set; }

        [ForeignKey("PNCVendeurId")]
        public PNC? PNCVendeur { get; set; }

        [Display(Name = "Taux de Change Appliqué")]
        public decimal TauxChangeApplique { get; set; } = 1;

        [Display(Name = "Chiffre d'Affaires (EUR)")]
        public decimal ChiffreAffairesEUR { get; set; } = 0;

        [Display(Name = "Montant Encaissé (TND)")]
        public decimal MontantEncaisseTND { get; set; } = 0;

        [Display(Name = "Montant Encaissé Réel (Déclaré)")]
        public decimal MontantEncaisseReel { get; set; } = 0;

        [StringLength(50)]
        public string Statut { get; set; } = "Saisi"; // Ex: "Saisi", "Contrôlé", "Clôturé"

        // Relation One-to-Many avec les lignes de vente
        public ICollection<LigneVente> Lignes { get; set; } = new List<LigneVente>();
    }
}
