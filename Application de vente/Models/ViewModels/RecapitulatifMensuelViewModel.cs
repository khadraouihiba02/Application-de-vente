using System.ComponentModel.DataAnnotations;
using ApplicationDeVente.Models;

namespace ApplicationDeVente.Models.ViewModels
{
    public class RecapitulatifMensuelViewModel
    {
        [Display(Name = "Mois")]
        public int Mois { get; set; } = DateTime.Today.Month;

        [Display(Name = "Année")]
        public int Annee { get; set; } = DateTime.Today.Year;

        public string NomMois { get; set; } = string.Empty;

        // KPI Globaux
        public decimal TotalVentesEUR { get; set; } = 0;
        public decimal TotalVentesTND { get; set; } = 0;
        public decimal TotalFRSEUR { get; set; } = 0;
        public decimal TotalEcartEUR { get; set; } = 0;

        public int NombreVols { get; set; } = 0;
        public int NombreEtatsVentes { get; set; } = 0;
        public int NombreEtatsOffres { get; set; } = 0;
        public int NombreControlesConformes { get; set; } = 0;
        public int NombreControlesEcarts { get; set; } = 0;
        public int NombreEnAttenteFRS { get; set; } = 0;

        // Redevance Mensuelle Catering
        public int TotalPassagersEstimes { get; set; } = 0;
        public decimal RedevanceMethode1 { get; set; } = 0; // Passagers * 1.10 €
        public decimal RedevanceMethode2 { get; set; } = 0; // 42% * CA Réel
        public decimal RedevanceRetenue { get; set; } = 0;  // Max(M1, M2)
        public string MethodeRetenueNom { get; set; } = string.Empty;

        // Listes détaillées pour les tableaux récapitulatifs
        public List<ItemRecapVente> VentesDetails { get; set; } = new();
        public List<ItemRecapOffre> OffresDetails { get; set; } = new();
    }

    public class ItemRecapVente
    {
        public int EtatId { get; set; }
        public string NumeroFeuilleLigne { get; set; } = string.Empty;
        public DateTime DateVol { get; set; }
        public string VolInfo { get; set; } = string.Empty;
        public string PNCVendeurNom { get; set; } = string.Empty;
        public decimal MontantEURPNC { get; set; }
        public decimal MontantEncaisseTND { get; set; }
        public decimal? MontantEURFRS { get; set; }
        public decimal? EcartEUR { get; set; }
        public string StatutPNC { get; set; } = "Saisi";
        public string StatutControle { get; set; } = "Non saisi"; // "Conforme", "Écart", "En attente", "Non saisi"
    }

    public class ItemRecapOffre
    {
        public int EtatId { get; set; }
        public string NumeroFeuilleLigne { get; set; } = string.Empty;
        public DateTime DateVol { get; set; }
        public string VolInfo { get; set; } = string.Empty;
        public int TotalDotationPNC { get; set; }
        public int TotalConsommePNC { get; set; }
        public int TotalConsommeFRS { get; set; }
        public decimal ValeurTotaleEUR { get; set; }
        public string StatutControle { get; set; } = "Non saisi";
    }
}
