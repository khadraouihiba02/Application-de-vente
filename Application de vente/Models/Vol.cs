using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class Vol
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro de vol est obligatoire.")]
        [StringLength(20)]
        [Display(Name = "Numéro de Vol")]
        public string NumeroVol { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'origine est obligatoire.")]
        [StringLength(10)]
        public string Origine { get; set; } = string.Empty;

        [Required(ErrorMessage = "La destination est obligatoire.")]
        [StringLength(10)]
        public string Destination { get; set; } = string.Empty;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        public string Trajet => $"{NumeroVol} ({Origine} - {Destination})";
    }
}
