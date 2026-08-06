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

        [Required(ErrorMessage = "Le montant encaissé réel est obligatoire.")]
        [Display(Name = "Montant Encaissé (TND)")]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal MontantEncaisseReel { get; set; } = 0;

        // ── Listes pour les menus déroulants ──
        public List<SelectListItem> VolsDisponibles { get; set; } = new();
        
        // L'agent sélectionne les Crews (équipage) du vol - puis choisit le vendeur parmi eux
        [Display(Name = "Équipage du Vol (Crews)")]
        public List<int> CrewIds { get; set; } = new List<int>();
        public List<SelectListItem> TousPNCs { get; set; } = new(); // Tous les PNC disponibles
        public List<SelectListItem> PNCsDisponibles { get; set; } = new(); // Non utilisé (gardé pour compatibilité)

        // ── Grille de saisie des articles ──
        public List<LigneSaisieArticle> LignesArticles { get; set; } = new();

        // ── Grille de saisie des offres ──
        public bool InclureOffres { get; set; } = false;
        public List<LigneSaisieOffre> LignesOffres { get; set; } = new();
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

    public class LigneSaisieOffre
    {
        public int ArticleId { get; set; }
        public string CodeArticle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;

        [Display(Name = "Dotation")]
        public int QuantiteDotation { get; set; } = 0;

        [Display(Name = "Complément")]
        public int QuantiteCompl { get; set; } = 0;

        [Display(Name = "Qté Offerte")]
        public int QuantiteOfferte { get; set; } = 0;

        [Display(Name = "Prix Promo EUR")]
        public decimal PrixUnitairePromoEUR { get; set; } = 0;
    }
}
