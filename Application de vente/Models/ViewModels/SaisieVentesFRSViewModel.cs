using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApplicationDeVente.Models.ViewModels
{
    public class SaisieVentesFRSViewModel
    {
        [Required(ErrorMessage = "Le numéro d'état FRS est obligatoire.")]
        [Display(Name = "N° État FRS")]
        public string NumeroEtat { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de réception est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Réception")]
        public DateTime DateReception { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Veuillez sélectionner l'état des ventes PNC associé.")]
        [Display(Name = "État des Ventes PNC")]
        public int EtatDesVentesId { get; set; }

        [Required(ErrorMessage = "Le montant FRS est obligatoire.")]
        [Display(Name = "Montant FRS (EUR)")]
        public decimal MontantFRS { get; set; } = 0;

        // Liste des états des ventes PNC disponibles pour la liste déroulante
        public List<SelectListItem> EtatsPNCDisponibles { get; set; } = new();

        // Grille de saisie des articles FRS
        public List<LigneSaisieVenteFRS> LignesArticles { get; set; } = new();
    }

    public class LigneSaisieVenteFRS
    {
        public string CodeArticle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        
        [Display(Name = "Quantité Vendue FRS")]
        public int QuantiteVendueFRS { get; set; } = 0;

        [Display(Name = "Prix Unitaire FRS")]
        public decimal PrixUnitaireFRS { get; set; } = 0;
    }
}
