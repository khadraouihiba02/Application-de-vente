using ApplicationDeVente.Models;

namespace ApplicationDeVente.Models.ViewModels
{
    // ViewModel pour le Dashboard DCF
    public class DCFDashboardViewModel
    {
        public int NbFacturesEnAttente { get; set; }
        public int NbFacturesValidees { get; set; }
        public int NbBordereauxATraiter { get; set; }
        public int NbDocumentsArchives { get; set; }
        public List<Facture> DernieresFactures { get; set; } = new();
    }

    // ViewModel pour un Trou de Caisse
    public class ItemTrouDeCaisse
    {
        public int EtatVenteId { get; set; }
        public string NumeroFL { get; set; } = string.Empty;
        public DateTime DateVol { get; set; }
        public string MatriculePNC { get; set; } = string.Empty;
        public string NomPNC { get; set; } = string.Empty;
        public decimal MontantTheorique { get; set; }
        public decimal MontantReel { get; set; }
        public decimal Deficit => MontantTheorique - MontantReel;
    }

    // ViewModel pour la page Trous de Caisse
    public class TrousDeCaisseViewModel
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string NomMois => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Mois);
        public List<ItemTrouDeCaisse> Trous { get; set; } = new();
        public decimal TotalDeficit => Trous.Sum(t => t.Deficit);
    }

    // ViewModel pour un Bordereau de Règlement
    public class BordereauViewModel
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string NomMois => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Mois);
        public string NumeroBordereau { get; set; } = string.Empty;
        public DateTime DateGeneration { get; set; } = DateTime.Today;
        public List<Facture> FacturesIncluses { get; set; } = new();
        public decimal TotalMontant => FacturesIncluses.Sum(f => f.Montant);
    }

    // ViewModel pour la page Archivage
    public class ArchivageViewModel
    {
        public List<ArchiveDocument> Documents { get; set; } = new();
    }

    // Représente un document archivé
    public class ArchiveDocument
    {
        public int Id { get; set; }
        public string TypeDocument { get; set; } = string.Empty; // "Bordereau", "État Commission", "Redevance"
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string NomMois => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Mois);
        public DateTime DateArchivage { get; set; }
        public string ArchivedBy { get; set; } = string.Empty;
        public string Statut { get; set; } = "Archivé";
    }
}
