using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApplicationDeVente.Models.ViewModels
{
    public class SaisieVentesViewModel
    {
        // ── En-tête de la Feuille de Ligne ──
        [Required(ErrorMessage = "Le numéro de Feuille de Ligne est obligatoire.")]
        [Display(Name = "N° Feuille de Ligne (FL)")]
        public string NumeroFeuilleLigne { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date du Vol")]
        public DateTime DateVol { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Veuillez sélectionner un vol.")]
        [Display(Name = "Vol")]
        public int VolId { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner le PNC vendeur.")]
        [Display(Name = "PNC Vendeur")]
        public int PNCVendeurId { get; set; }

        public decimal TauxChangeApplique { get; set; } = 1;

        // ── Listes pour les menus déroulants ──
        public List<SelectListItem> VolsDisponibles { get; set; } = new();
        public List<SelectListItem> PNCsDisponibles { get; set; } = new();

        // ── Grille de saisie des articles ──
        public List<LigneSaisieArticle> LignesArticles { get; set; } = new();
    }

    public class LigneSaisieArticle
    {
        public int ArticleId { get; set; }
        public string CodeArticle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal PrixUnitaireEUR { get; set; }

        public int QuantiteDotation { get; set; } = 0;
        public int QuantiteCompl { get; set; } = 0;
        public int QuantiteVendue { get; set; } = 0;
    }
}
