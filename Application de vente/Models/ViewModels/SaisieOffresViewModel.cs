using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApplicationDeVente.Models.ViewModels
{
    public class SaisieOffresViewModel
    {
        // ── En-tête de la Feuille de Ligne ──
        [Required(ErrorMessage = "Le numéro de Feuille de Ligne est obligatoire.")]
        [Display(Name = "N° Feuille de Ligne (FL)")]
        public string NumeroFeuilleLigne { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date du Vol")]
        public DateTime DateVol { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Veuillez sélectionner au moins un vol.")]
        [Display(Name = "Vols de la rotation")]
        public int[] VolsIds { get; set; } = Array.Empty<int>();

        public decimal TauxChangeApplique { get; set; } = 1;

        // ── Listes pour les menus déroulants ──
        public List<SelectListItem> VolsDisponibles { get; set; } = new();

        // ── Grille de saisie des articles ──
        public List<LigneSaisieOffre> LignesArticles { get; set; } = new();
    }

    public class LigneSaisieOffre
    {
        public int ArticleId { get; set; }
        public string CodeArticle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal PrixCatalogueEUR { get; set; }   // Prix normal (lecture seule)

        public int QuantiteDotation { get; set; } = 0;
        public int QuantiteCompl { get; set; } = 0;
        public int QuantiteOfferte { get; set; } = 0;

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal PrixUnitairePromoEUR { get; set; } = 0; // 0 = gratuit, sinon prix réduit
    }
}
