using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using ApplicationDeVente.Models.ViewModels;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "DCF")]
    public class DCFController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DCFController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // =====================================================================
        // DASHBOARD
        // =====================================================================
        public async Task<IActionResult> Dashboard()
        {
            var nbRedevances = await _db.Redevances.CountAsync();
            var nbMoisPayes = await _db.Factures.Where(f => f.Statut == "Payée").Select(f => new { f.DateFacture.Month, f.DateFacture.Year }).Distinct().CountAsync();

            var vm = new DCFDashboardViewModel
            {
                NbFacturesEnAttente  = await _db.Factures.CountAsync(f => f.Statut == "En attente"),
                NbFacturesValidees   = await _db.Factures.CountAsync(f => f.Statut == "Validée"),
                NbBordereauxATraiter = await _db.Factures.CountAsync(f => f.Statut == "Validée"),
                NbDocumentsArchives  = nbRedevances + nbMoisPayes,
                DernieresFactures    = await _db.Factures
                                            .OrderByDescending(f => f.DateFacture)
                                            .Take(5)
                                            .ToListAsync()
            };

            ViewData["Title"] = "Tableau de bord — DCF";
            return View(vm);
        }

        // =====================================================================
        // TROUS DE CAISSE
        // =====================================================================
        public async Task<IActionResult> TrousDeCaisse(int mois = 0, int annee = 0)
        {
            if (mois == 0) mois = DateTime.Today.Month;
            if (annee == 0) annee = DateTime.Today.Year;

            // Récupérer tous les états de ventes du mois où il y a un déficit
            var etats = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Where(e => e.DateVol.Month == mois && e.DateVol.Year == annee
                         && e.MontantEncaisseReel < e.MontantEncaisseTND)
                .ToListAsync();

            var trous = etats.Select(e => new ItemTrouDeCaisse
            {
                EtatVenteId    = e.Id,
                NumeroFL       = e.NumeroFeuilleLigne,
                DateVol        = e.DateVol,
                MatriculePNC   = e.PNCVendeur?.TLC ?? "N/A",
                NomPNC         = e.PNCVendeur != null
                                    ? $"{e.PNCVendeur.name} {e.PNCVendeur.First_name}"
                                    : "Inconnu",
                MontantTheorique = e.MontantEncaisseTND,
                MontantReel      = e.MontantEncaisseReel
            }).ToList();

            var vm = new TrousDeCaisseViewModel
            {
                Mois  = mois,
                Annee = annee,
                Trous = trous
            };

            ViewData["Title"] = "Trous de Caisse";
            return View(vm);
        }

        // =====================================================================
        // BORDEREAUX DE RÈGLEMENT
        // =====================================================================
        public async Task<IActionResult> Bordereaux(int mois = 0, int annee = 0)
        {
            if (mois == 0) mois = DateTime.Today.Month;
            if (annee == 0) annee = DateTime.Today.Year;

            // Les factures validées par Catering (= "Bon à payer") du mois sélectionné
            var facturesValidees = await _db.Factures
                .Where(f => f.Statut == "Validée"
                         && f.DateFacture.Month == mois
                         && f.DateFacture.Year == annee)
                .OrderBy(f => f.Type)
                .ToListAsync();

            var vm = new BordereauViewModel
            {
                Mois             = mois,
                Annee            = annee,
                NumeroBordereau  = $"BRD-{annee}{mois:D2}-{DateTime.Now:HHmmss}",
                DateGeneration   = DateTime.Today,
                FacturesIncluses = facturesValidees
            };

            ViewData["Title"] = "Bordereaux de Règlement";
            return View(vm);
        }

        // Action pour marquer les factures du bordereau comme "Payée"
        [HttpPost]
        public async Task<IActionResult> ValiderBordereau(int mois, int annee)
        {
            var factures = await _db.Factures
                .Where(f => f.Statut == "Validée"
                         && f.DateFacture.Month == mois
                         && f.DateFacture.Year == annee)
                .ToListAsync();

            foreach (var f in factures)
            {
                f.Statut = "Payée";
            }

            await _db.SaveChangesAsync();
            TempData["Succes"] = $"{factures.Count} facture(s) marquée(s) comme Payée(s). Bordereau clôturé.";
            return RedirectToAction(nameof(Bordereaux), new { mois, annee });
        }

        // =====================================================================
        // ARCHIVAGE
        // =====================================================================
        public async Task<IActionResult> Archivage()
        {
            // Regrouper les redevances sauvegardées comme "documents archivables"
            var redevances = await _db.Redevances
                .OrderByDescending(r => r.Annee)
                .ThenByDescending(r => r.Mois)
                .ToListAsync();

            var docs = redevances.Select(r => new ArchiveDocument
            {
                Id            = r.Id,
                TypeDocument  = "Redevance Mensuelle",
                Mois          = r.Mois,
                Annee         = r.Annee,
                DateArchivage = new DateTime(r.Annee, r.Mois, 1),
                ArchivedBy    = "Direction Catering",
                Statut        = "Archivé"
            }).ToList();

            // Ajouter les mois avec des factures payées
            var moisPayes = await _db.Factures
                .Where(f => f.Statut == "Payée")
                .Select(f => new { f.DateFacture.Month, f.DateFacture.Year })
                .Distinct()
                .ToListAsync();

            foreach (var mp in moisPayes)
            {
                if (!docs.Any(d => d.Mois == mp.Month && d.Annee == mp.Year && d.TypeDocument == "Bordereau de Règlement"))
                {
                    docs.Add(new ArchiveDocument
                    {
                        TypeDocument  = "Bordereau de Règlement",
                        Mois          = mp.Month,
                        Annee         = mp.Year,
                        DateArchivage = DateTime.Today,
                        ArchivedBy    = "Direction DCF",
                        Statut        = "Archivé"
                    });
                }
            }

            var vm = new ArchivageViewModel
            {
                Documents = docs.OrderByDescending(d => d.Annee).ThenByDescending(d => d.Mois).ToList()
            };

            ViewData["Title"] = "Archivage Documents";
            return View(vm);
        }
    }
}
