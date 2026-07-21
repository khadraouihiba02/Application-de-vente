using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class RedevanceMensuelle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mois")]
        public int Mois { get; set; }

        [Required]
        [Display(Name = "Année")]
        public int Annee { get; set; }

        [Display(Name = "Nombre de passagers transportés")]
        public int NombrePassagers { get; set; }

        [Display(Name = "Chiffre d'Affaires Total (EUR)")]
        public decimal ChiffreAffairesTotal { get; set; }

        [Display(Name = "Montant Minimum Garanti (EUR)")]
        public decimal MontantMinGaranti { get; set; }

        [Display(Name = "Montant au Pourcentage (EUR)")]
        public decimal MontantPourcentage { get; set; }

        [Required]
        [Display(Name = "Montant Retenu (EUR)")]
        public decimal MontantRetenu { get; set; }

        [StringLength(100)]
        [Display(Name = "Méthode Appliquée")]
        public string MethodeAppliquee { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Statut de Facturation")]
        public string StatutFacturation { get; set; } = "Non facturée"; // "Non facturée", "Demande envoyée", "Facturée"

        [Display(Name = "Date de Calcul")]
        public DateTime DateCalcul { get; set; } = DateTime.Now;
    }
}
