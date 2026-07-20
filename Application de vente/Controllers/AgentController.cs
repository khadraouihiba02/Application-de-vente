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
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.PNCVendeur)
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToListAsync();

            // Charger les 5 dernières saisies d'offres
            vm.DerniersEtatsOffres = await _db.EtatsDesOffres
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
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
            var aujourdhui = DateTime.Today;

            // Vols disponibles (actifs, à partir de la date du jour ou récents)
            vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif)
                .Select(v => new SelectListItem 
                { 
                    Value = v.Id.ToString(), 
                    Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination}) - {v.DateVol:dd/MM/yyyy}" 
                })
                .ToListAsync();

            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            vm.TauxChangeApplique = tauxActif?.Taux ?? 3.4000m;

            // La grille des articles est vide au départ
            vm.LignesArticles = new List<LigneSaisieArticle>();
            vm.PNCsDisponibles = new List<SelectListItem>(); // Rempli via AJAX

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
                .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination}) - {v.DateVol:dd/MM/yyyy}" })
                .ToListAsync();

            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            vm.TauxChangeApplique = tauxActif?.Taux ?? 3.4000m;

            vm.LignesArticles = new List<LigneSaisieOffre>();

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirOffres(SaisieOffresViewModel vm)
        {
            if (!ModelState.IsValid || vm.VolsIds == null || vm.VolsIds.Length == 0)
            {
                vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.NumeroVol} ({v.Origine} - {v.Destination}) - {v.DateVol:dd/MM/yyyy}" }).ToListAsync();
                if(vm.LignesArticles == null) vm.LignesArticles = new List<LigneSaisieOffre>();
                return View(vm);
            }

            // On ne garde que les lignes valides saisies par l'agent
            var lignesFiltrees = vm.LignesArticles.Where(l => l.ArticleId > 0).ToList();

            decimal totalEur = lignesFiltrees.Sum(l => l.QuantiteOfferte * l.PrixUnitairePromoEUR);

            var etatOffres = new EtatDesOffres
            {
                NumeroFeuilleLigne = vm.NumeroFeuilleLigne,
                DateVol = vm.DateVol,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantEncaisseTND = totalEur * vm.TauxChangeApplique,
                Statut = "Saisi"
            };

            foreach (var volId in vm.VolsIds)
            {
                etatOffres.VolsList.Add(new EtatDesOffresVol { VolId = volId });
            }

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

            TempData["Succes"] = $"L'état des offres pour la FL {vm.NumeroFeuilleLigne} a été enregistré avec succès.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ── Endpoints API pour l'interface dynamique ─────────────────

        [HttpGet]
        public async Task<IActionResult> GetCrewsByVols(string volsIds)
        {
            if (string.IsNullOrEmpty(volsIds))
                return Json(new List<object>());

            var ids = volsIds.Split(',').Select(int.Parse).ToList();
            
            var crews = await _db.CrewAssignments
                .Include(c => c.PNC)
                .Where(c => ids.Contains(c.VolId) && c.PNC.Actif)
                .Select(c => new { 
                    id = c.PNC.Id, 
                    texte = $"{c.PNC.Matricule} - {c.PNC.Nom} {c.PNC.Prenom} ({c.Rank})" 
                })
                .Distinct()
                .ToListAsync();

            return Json(crews);
        }

        [HttpGet]
        public async Task<IActionResult> RechercherArticle(string query)
        {
            if (string.IsNullOrEmpty(query)) return Json(new List<object>());

            var term = query.ToLower();
            var articles = await _db.Articles
                .Where(a => a.CodeArticle.ToLower().Contains(term) || a.NomArticle.ToLower().Contains(term))
                .Take(10)
                .Select(a => new {
                    id = a.Id,
                    code = a.CodeArticle,
                    designation = a.NomArticle,
                    prix = a.PrixUnitaire
                })
                .ToListAsync();

            return Json(articles);
        }
    }
}
