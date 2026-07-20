using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using ApplicationDeVente.Models;

namespace ApplicationDeVente.Models.ViewModels
{
    public class VolViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro de vol est obligatoire.")]
        [StringLength(10)]
        [Display(Name = "Numéro de Vol")]
        public string NumeroVol { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'aéroport d'origine est obligatoire.")]
        [StringLength(10)]
        public string Origine { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'aéroport de destination est obligatoire.")]
        [StringLength(10)]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date du Vol")]
        public DateTime DateVol { get; set; } = DateTime.Today;

        public bool Actif { get; set; } = true;

        [Display(Name = "Équipage (PNC) affecté à ce vol")]
        public List<int> SelectedPncIds { get; set; } = new List<int>();

        // Liste pour peupler le menu déroulant Select2
        public IEnumerable<SelectListItem>? PncsDisponibles { get; set; }
    }
}
