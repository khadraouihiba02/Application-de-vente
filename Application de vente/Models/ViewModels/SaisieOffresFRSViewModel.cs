using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApplicationDeVente.Models.ViewModels
{
    public class SaisieOffresFRSViewModel
    {
        [Required(ErrorMessage = "Le numéro d'état FRS est obligatoire.")]
        [Display(Name = "N° État FRS")]
        public string NumeroEtat { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de réception est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Réception")]
        public DateTime DateReception { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Veuillez sélectionner l'état des offres PNC associé.")]
        [Display(Name = "État des Offres PNC")]
        public int EtatDesOffresId { get; set; }

        // Liste des états des offres PNC disponibles pour la liste déroulante
        public List<SelectListItem> EtatsPNCDisponibles { get; set; } = new();

        // Grille de saisie des offres FRS
        public List<LigneSaisieOffreFRS> LignesArticles { get; set; } = new();
    }

    public class LigneSaisieOffreFRS
    {
        public string CodeArticle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;

        [Display(Name = "Dotation Initiale FRS")]
        public int DotationInitialeFRS { get; set; } = 0;

        [Display(Name = "Quantité Restante FRS")]
        public int QuantiteRestanteFRS { get; set; } = 0;

        [Display(Name = "Quantité Consommée FRS")]
        public int QuantiteConsommeeFRS { get; set; } = 0;
    }
}
