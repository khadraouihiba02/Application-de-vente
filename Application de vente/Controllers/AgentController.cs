using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using ApplicationDeVente.Models.ViewModels;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AgentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Agent";

            var debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var vm = new AgentDashboardViewModel();

            // Compter les statistiques réelles
            vm.VolsSaisisCeMois = await _db.EtatsDesVentes
                .CountAsync(e => e.DateVol >= debutMois && e.DateVol <= finMois);

            vm.EtatsEnAttente = await _db.EtatsDesVentes
                .CountAsync(e => e.Statut == "Saisi");

            vm.EtatsValides = await _db.EtatsDesVentes
                .CountAsync(e => e.Statut == "Clôturé" || e.Statut == "Contrôlé");

            // Calcul commission estimée (ex: 5% du chiffre d'affaires TND pour le PNC)
            var totalTndSaisi = await _db.EtatsDesVentes
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .SumAsync(e => e.MontantEncaisseTND);
            vm.CommissionEstimee = totalTndSaisi * 0.05m; // 5% estimé

            // Charger les 5 dernières saisies de ventes
            vm.DerniersEtats = await _db.EtatsDesVentes
                .Include(e => e.Vol)
                .Include(e => e.PNCVendeur)
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToListAsync();

            // Charger les 5 dernières saisies d'offres
            vm.DerniersEtatsOffres = await _db.EtatsDesOffres
                .Include(e => e.Vol)
                .Include(e => e.PNCVendeur)
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> SaisirVentes()
        {
            var vm = new SaisieVentesViewModel();

            // Remplir les listes déroulantes
            vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif)
                .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination})" })
                .ToListAsync();

            vm.PNCsDisponibles = await _db.PNCs.Where(p => p.Actif)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Matricule} - {p.Nom} {p.Prenom}" })
                .ToListAsync();

            // Obtenir le taux de change actif (EUR vers TND par exemple) pour la date d'aujourd'hui
            var aujourdhui = DateTime.Today;
            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
                
            if (tauxActif != null)
            {
                vm.TauxChangeApplique = tauxActif.Taux;
            }
            else
            {
                // Taux par défaut si non paramétré
                vm.TauxChangeApplique = 3.4000m; 
            }

            // Précharger le catalogue d'articles actifs aujourd'hui
            var articles = await _db.Articles
                .Where(a => aujourdhui >= a.DateDebut && aujourdhui <= a.DateFin)
                .OrderBy(a => a.NomArticle)
                .ToListAsync();
                
            foreach (var article in articles)
            {
                vm.LignesArticles.Add(new LigneSaisieArticle
                {
                    ArticleId = article.Id,
                    CodeArticle = article.CodeArticle,
                    Designation = article.NomArticle,
                    PrixUnitaireEUR = article.PrixUnitaire
                });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirVentes(SaisieVentesViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Recharger les listes en cas d'erreur
                vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination})" }).ToListAsync();
                vm.PNCsDisponibles = await _db.PNCs.Where(p => p.Actif).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Matricule} - {p.Nom} {p.Prenom}" }).ToListAsync();
                return View(vm);
            }

            // Calcul du total
            decimal totalEur = 0;
            var lignesFiltrees = vm.LignesArticles.Where(l => l.QuantiteVendue > 0).ToList();

            foreach (var ligne in lignesFiltrees)
            {
                totalEur += (ligne.QuantiteVendue * ligne.PrixUnitaireEUR);
            }

            var etatVentes = new EtatDesVentes
            {
                NumeroFeuilleLigne = vm.NumeroFeuilleLigne,
                DateVol = vm.DateVol,
                VolId = vm.VolId,
                PNCVendeurId = vm.PNCVendeurId,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantEncaisseTND = totalEur * vm.TauxChangeApplique,
                Statut = "Saisi"
            };

            foreach (var ligne in lignesFiltrees)
            {
                etatVentes.Lignes.Add(new LigneVente
                {
                    ArticleId = ligne.ArticleId,
                    QuantiteDotation = ligne.QuantiteDotation,
                    QuantiteCompl = ligne.QuantiteCompl,
                    QuantiteVendue = ligne.QuantiteVendue,
                    PrixUnitaireEUR = ligne.PrixUnitaireEUR
                });
            }

            _db.EtatsDesVentes.Add(etatVentes);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des ventes pour la FL {vm.NumeroFeuilleLigne} a été enregistré avec succès (Total : {totalEur:F2} €).";
            return RedirectToAction(nameof(Dashboard));
        }

        // ── Saisie des Offres à bord ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SaisirOffres()
        {
            var vm = new SaisieOffresViewModel();
            var aujourdhui = DateTime.Today;

            vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif)
                .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination})" })
                .ToListAsync();

            vm.PNCsDisponibles = await _db.PNCs.Where(p => p.Actif)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Matricule} - {p.Nom} {p.Prenom}" })
                .ToListAsync();

            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            vm.TauxChangeApplique = tauxActif?.Taux ?? 3.4000m;

            var articles = await _db.Articles
                .Where(a => aujourdhui >= a.DateDebut && aujourdhui <= a.DateFin)
                .OrderBy(a => a.NomArticle)
                .ToListAsync();

            foreach (var article in articles)
            {
                vm.LignesArticles.Add(new LigneSaisieOffre
                {
                    ArticleId = article.Id,
                    CodeArticle = article.CodeArticle,
                    Designation = article.NomArticle,
                    PrixCatalogueEUR = article.PrixUnitaire,
                    PrixUnitairePromoEUR = 0 // gratuit par défaut
                });
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirOffres(SaisieOffresViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination})" }).ToListAsync();
                vm.PNCsDisponibles = await _db.PNCs.Where(p => p.Actif).Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Matricule} - {p.Nom} {p.Prenom}" }).ToListAsync();
                return View(vm);
            }

            // On ne garde que les lignes avec une quantité offerte > 0
            var lignesFiltrees = vm.LignesArticles.Where(l => l.QuantiteOfferte > 0).ToList();

            decimal totalEur = lignesFiltrees.Sum(l => l.QuantiteOfferte * l.PrixUnitairePromoEUR);

            var etatOffres = new EtatDesOffres
            {
                NumeroFeuilleLigne = vm.NumeroFeuilleLigne,
                DateVol = vm.DateVol,
                VolId = vm.VolId,
                PNCVendeurId = vm.PNCVendeurId,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantEncaisseTND = totalEur * vm.TauxChangeApplique,
                Statut = "Saisi"
            };

            foreach (var ligne in lignesFiltrees)
            {
                etatOffres.Lignes.Add(new LigneOffre
                {
                    ArticleId = ligne.ArticleId,
                    QuantiteDotation = ligne.QuantiteDotation,
                    QuantiteCompl = ligne.QuantiteCompl,
                    QuantiteOfferte = ligne.QuantiteOfferte,
                    PrixUnitairePromoEUR = ligne.PrixUnitairePromoEUR
                });
            }

            _db.EtatsDesOffres.Add(etatOffres);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des offres pour la FL {vm.NumeroFeuilleLigne} a été enregistré avec succès ({lignesFiltrees.Count} article(s) offert(s)).";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
