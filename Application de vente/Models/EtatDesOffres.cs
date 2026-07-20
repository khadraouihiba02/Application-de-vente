using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesOffres
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
        public ICollection<EtatDesOffresVol> VolsList { get; set; } = new List<EtatDesOffresVol>();


        [Display(Name = "Taux de Change Appliqué")]
        public decimal TauxChangeApplique { get; set; } = 1;

        [Display(Name = "Valeur Offres (EUR)")]
        public decimal ChiffreAffairesEUR { get; set; } = 0;

        [Display(Name = "Valeur Offres (TND)")]
        public decimal MontantEncaisseTND { get; set; } = 0;

        [StringLength(50)]
        public string Statut { get; set; } = "Saisi";

        public ICollection<LigneOffre> Lignes { get; set; } = new List<LigneOffre>();
    }
}
