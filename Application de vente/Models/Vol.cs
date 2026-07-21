using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class Vol
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro de vol est obligatoire.")]
        [StringLength(20)]
        [Display(Name = "Numéro de Vol (FN_NUMBER)")]
        public string FN_NUMBER { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'origine est obligatoire.")]
        [StringLength(10)]
        [Display(Name = "Origine (DEP_AP_ACTUAL)")]
        public string DEP_AP_ACTUAL { get; set; } = string.Empty;

        [Required(ErrorMessage = "La destination est obligatoire.")]
        [StringLength(10)]
        [Display(Name = "Destination (ARR_AP_ACTUAL)")]
        public string ARR_AP_ACTUAL { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date du Vol (DAY_OF_ORIGIN)")]
        public DateTime DAY_OF_ORIGIN { get; set; } = DateTime.Today;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        public string Trajet => $"{FN_NUMBER} ({DEP_AP_ACTUAL} - {ARR_AP_ACTUAL})";
    }
}
