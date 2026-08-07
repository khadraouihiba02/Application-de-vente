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

        // ── Méthodes privées utilitaires ─────────────────────────────────────

        private async Task<List<SelectListItem>> GetVolsDisponiblesAsync()
        {
            return await _db.Vols
                .Where(v => v.Actif)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.FN_NUMBER} ({v.DEP_AP_ACTUAL} - {v.ARR_AP_ACTUAL})"
                })
                .ToListAsync();
        }

        private async Task<decimal> GetTauxChangeActifAsync()
        {
            var aujourdhui = DateTime.Today;
            var taux = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            return taux?.Taux ?? 3.4000m;
        }

        private async Task<List<SelectListItem>> GetEtatsPNCDisponiblesAsync()
        {
            var dejaSaisis = await _db.EtatsDesVentesFRS.Select(f => f.EtatDesVentesId).ToListAsync();
            var etats = await _db.EtatsDesVentes
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Where(e => !dejaSaisis.Contains(e.Id))
                .ToListAsync();

            return etats.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"FL: {e.NumeroFeuilleLigne} | Vol: {e.VolsList.FirstOrDefault()?.Vol?.FN_NUMBER ?? "N/A"} | Date: {e.DateVol:dd/MM/yyyy}"
            }).ToList();
        }

        private static FileContentResult BuildCsvResult(System.Text.StringBuilder sb, string fileName)
        {
            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
            return new FileContentResult(bytes, "text/csv") { FileDownloadName = fileName };
        }

        private static void AjouterEtatDesOffres(EtatDesVentes etatVentes, SaisieVentesViewModel vm, ApplicationDbContext db)
        {
            if (vm.LignesOffres == null || !vm.LignesOffres.Any(l => l.ArticleId > 0)) return;

            var lignesOffresFiltrees = vm.LignesOffres.Where(l => l.ArticleId > 0).ToList();
            decimal totalOffresEur = lignesOffresFiltrees.Sum(l => l.QuantiteOfferte * l.PrixUnitairePromoEUR);

            var etatOffres = new EtatDesOffres
            {
                NumeroFeuilleLigne = vm.NumeroFeuilleLigne,
                DateVol = vm.DateVol,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalOffresEur,
                MontantEncaisseTND = totalOffresEur * vm.TauxChangeApplique,
                Statut = "Saisi"
            };

            etatOffres.VolsList.Add(new EtatDesOffresVol { VolId = vm.VolId });

            foreach (var ligne in lignesOffresFiltrees)
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
            db.EtatsDesOffres.Add(etatOffres);
        }

        private async Task AjouterEtatDesOffresFRSAsync(SaisieVentesFRSViewModel vm)
        {
            if (vm.LignesOffres == null || !vm.LignesOffres.Any(l => l.DotationInitialeFRS > 0 || l.QuantiteRestanteFRS > 0))
                return;

            var etatVentesPNC = await _db.EtatsDesVentes.FindAsync(vm.EtatDesVentesId);
            if (etatVentesPNC == null) return;

            var etatOffresPNC = await _db.EtatsDesOffres
                .FirstOrDefaultAsync(o => o.NumeroFeuilleLigne == etatVentesPNC.NumeroFeuilleLigne && o.DateVol == etatVentesPNC.DateVol);
            if (etatOffresPNC == null) return;

            var lignesOffresFiltrees = vm.LignesOffres
                .Where(l => l.DotationInitialeFRS > 0 || l.QuantiteRestanteFRS > 0)
                .ToList();

            var etatOffresFRS = new EtatDesOffresFRS
            {
                NumeroEtat = vm.NumeroEtat,
                DateReception = vm.DateReception,
                EtatDesOffresId = etatOffresPNC.Id,
                StatutControle = "En attente"
            };

            foreach (var ligne in lignesOffresFiltrees)
            {
                int qteConsommee = Math.Max(0, ligne.DotationInitialeFRS - ligne.QuantiteRestanteFRS);
                etatOffresFRS.Lignes.Add(new LigneOffreFRS
                {
                    CodeArticle = ligne.CodeArticle,
                    NomArticle = ligne.Designation,
                    DotationInitialeFRS = ligne.DotationInitialeFRS,
                    QuantiteRestanteFRS = ligne.QuantiteRestanteFRS,
                    QuantiteConsommeeFRS = qteConsommee
                });
            }
            _db.EtatsDesOffresFRS.Add(etatOffresFRS);
        }

        // ── Actions publiques ──────────────────────────────────────────────────

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Agent";

            var debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var vm = new AgentDashboardViewModel
            {
                VolsSaisisCeMois = await _db.EtatsDesVentes
                    .CountAsync(e => e.DateVol >= debutMois && e.DateVol <= finMois),
                EtatsEnAttente = await _db.EtatsDesVentes
                    .CountAsync(e => e.Statut == "Saisi"),
                EtatsValides = await _db.EtatsDesVentes
                    .CountAsync(e => e.Statut == "Clôturé" || e.Statut == "Contrôlé"),
                DerniersEtats = await _db.EtatsDesVentes
                    .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                    .Include(e => e.PNCVendeur)
                    .OrderByDescending(e => e.Id).Take(5).ToListAsync(),
                DerniersEtatsOffres = await _db.EtatsDesOffres
                    .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                    .OrderByDescending(e => e.Id).Take(5).ToListAsync(),
                DerniersEtatsFRS = await _db.EtatsDesVentesFRS
                    .Include(e => e.EtatDesVentes).ThenInclude(ev => ev.VolsList).ThenInclude(ev => ev.Vol)
                    .OrderByDescending(e => e.Id).Take(5).ToListAsync(),
                DerniersEtatsOffresFRS = await _db.EtatsDesOffresFRS
                    .Include(e => e.EtatDesOffres).ThenInclude(ev => ev.VolsList).ThenInclude(ev => ev.Vol)
                    .OrderByDescending(e => e.Id).Take(5).ToListAsync()
            };

            var totalTndSaisi = await _db.EtatsDesVentes
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .SumAsync(e => e.MontantEncaisseTND);
            vm.CommissionEstimee = totalTndSaisi * 0.05m;

            return View(vm);
        }

        public IActionResult Index() => RedirectToAction("Dashboard");

        [HttpGet]
        public async Task<IActionResult> SaisirVentes()
        {
            var vm = new SaisieVentesViewModel
            {
                VolsDisponibles = await GetVolsDisponiblesAsync(),
                TauxChangeApplique = await GetTauxChangeActifAsync(),
                TousPNCs = new List<SelectListItem>(),
                LignesArticles = new List<LigneSaisieArticle>(),
                PNCsDisponibles = new List<SelectListItem>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirVentes(SaisieVentesViewModel vm)
        {
            if (!ModelState.IsValid || vm.VolId == 0)
            {
                vm.VolsDisponibles = await GetVolsDisponiblesAsync();
                vm.LignesArticles ??= new List<LigneSaisieArticle>();
                return View(vm);
            }

            var lignesFiltrees = (vm.LignesArticles ?? new List<LigneSaisieArticle>())
                .Where(l => l.QuantiteVendue > 0).ToList();
            decimal totalEur = lignesFiltrees.Sum(l => l.QuantiteVendue * l.PrixUnitaireEUR);

            var etatVentes = new EtatDesVentes
            {
                NumeroFeuilleLigne = vm.NumeroFeuilleLigne,
                DateVol = vm.DateVol,
                PNCVendeurId = vm.PNCVendeurId,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantEncaisseTND = totalEur * vm.TauxChangeApplique,
                MontantEncaisseReel = vm.MontantEncaisseReel,
                Statut = "Saisi"
            };

            etatVentes.VolsList.Add(new EtatDesVentesVol { VolId = vm.VolId });

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
            AjouterEtatDesOffres(etatVentes, vm, _db);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des ventes (et offres si saisies) pour la FL {vm.NumeroFeuilleLigne} a été enregistré avec succès.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> SaisirVentesFRS()
        {
            var vm = new SaisieVentesFRSViewModel
            {
                EtatsPNCDisponibles = await GetEtatsPNCDisponiblesAsync(),
                TauxChangeApplique = await GetTauxChangeActifAsync(),
                LignesArticles = new List<LigneSaisieVenteFRS>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirVentesFRS(SaisieVentesFRSViewModel vm)
        {
            if (!ModelState.IsValid || vm.EtatDesVentesId == 0)
            {
                vm.EtatsPNCDisponibles = await GetEtatsPNCDisponiblesAsync();
                vm.LignesArticles ??= new List<LigneSaisieVenteFRS>();
                return View(vm);
            }

            var lignesFiltrees = (vm.LignesArticles ?? new List<LigneSaisieVenteFRS>())
                .Where(l => l.QuantiteVendueFRS > 0).ToList();
            decimal totalEur = lignesFiltrees.Sum(l => l.QuantiteVendueFRS * l.PrixUnitaireFRS);

            var etatVentesFRS = new EtatDesVentesFRS
            {
                NumeroEtat = vm.NumeroEtat,
                DateReception = vm.DateReception,
                EtatDesVentesId = vm.EtatDesVentesId,
                MontantFRS = 0,
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantTheoriqueTND = totalEur * vm.TauxChangeApplique,
                MontantDeclareReelTND = vm.MontantDeclareReelTND,
                StatutControle = "En attente"
            };

            foreach (var ligne in lignesFiltrees)
            {
                etatVentesFRS.Lignes.Add(new LigneVenteFRS
                {
                    CodeArticle = ligne.CodeArticle,
                    NomArticle = ligne.Designation,
                    QuantiteVendueFRS = ligne.QuantiteVendueFRS,
                    PrixUnitaireFRS = ligne.PrixUnitaireFRS,
                    ValeurFRS = ligne.QuantiteVendueFRS * ligne.PrixUnitaireFRS
                });
            }

            _db.EtatsDesVentesFRS.Add(etatVentesFRS);
            await AjouterEtatDesOffresFRSAsync(vm);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des ventes FRS N° {vm.NumeroEtat} a été enregistré avec succès.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ── Endpoints API pour l'interface dynamique ─────────────────

        [HttpGet]
        public async Task<IActionResult> GetCrewsByVol(int volId)
        {
            if (volId == 0) return Json(new List<object>());

            var vol = await _db.Vols.FindAsync(volId);
            if (vol == null) return Json(new List<object>());

            var crews = await _db.PNCs
                .Where(p => p.FlightNumber == vol.FN_NUMBER && p.Day_of_origin.Date == vol.DAY_OF_ORIGIN.Date)
                .Select(p => new { id = p.Id, texte = $"{p.TLC} - {p.name} {p.First_name} ({p.Rank})" })
                .ToListAsync();

            return Json(crews);
        }

        [HttpGet]
        public async Task<IActionResult> GetVolsByDate(string date)
        {
            DateTime parsedDate;
            bool parsed = DateTime.TryParseExact(date, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out parsedDate);

            if (!parsed && !DateTime.TryParse(date, out parsedDate))
                return Json(new List<object>());

            var vols = await _db.Vols
                .Where(v => v.DAY_OF_ORIGIN.Date == parsedDate.Date)
                .Select(v => new
                {
                    id = v.Id,
                    texte = $"{v.FN_NUMBER} ({v.DEP_AP_ACTUAL} - {v.ARR_AP_ACTUAL}) - {v.DAY_OF_ORIGIN:dd/MM/yyyy}"
                })
                .ToListAsync();

            return Json(vols);
        }

        [HttpGet]
        public async Task<IActionResult> RechercherArticle(string query)
        {
            if (string.IsNullOrEmpty(query)) return Json(new List<object>());

            var term = query.ToLower();
            var articles = await _db.Articles
                .Where(a => a.CodeArticle.ToLower().Contains(term) || a.NomArticle.ToLower().Contains(term))
                .Take(10)
                .Select(a => new { id = a.Id, code = a.CodeArticle, designation = a.NomArticle, prix = a.PrixUnitaire })
                .ToListAsync();

            return Json(articles);
        }

        // ── Endpoints pour Détails et Exportation Dashboard ──────────

        [HttpGet]
        public async Task<IActionResult> GetDetailsVente(int id)
        {
            var etat = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            return Json(new
            {
                fl = etat.NumeroFeuilleLigne,
                date = etat.DateVol.ToString("dd/MM/yyyy"),
                vol = string.Join(" / ", etat.VolsList.Select(v => v.Vol?.FN_NUMBER)),
                pnc = etat.PNCVendeur != null ? $"{etat.PNCVendeur.name} {etat.PNCVendeur.First_name}" : "-",
                totalEur = etat.ChiffreAffairesEUR,
                totalTnd = etat.MontantEncaisseTND,
                statut = etat.Statut,
                lignes = etat.Lignes.Select(l => new
                {
                    code = l.Article?.CodeArticle ?? "N/A",
                    designation = l.Article?.NomArticle ?? "Inconnu",
                    dotation = l.QuantiteDotation,
                    complement = l.QuantiteCompl,
                    vendue = l.QuantiteVendue,
                    prixUnit = l.PrixUnitaireEUR,
                    total = l.QuantiteVendue * l.PrixUnitaireEUR
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsOffre(int id)
        {
            var etat = await _db.EtatsDesOffres
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            return Json(new
            {
                fl = etat.NumeroFeuilleLigne,
                date = etat.DateVol.ToString("dd/MM/yyyy"),
                vol = string.Join(" / ", etat.VolsList.Select(v => v.Vol?.FN_NUMBER)),
                valeurEur = etat.ChiffreAffairesEUR,
                statut = etat.Statut,
                lignes = etat.Lignes.Select(l => new
                {
                    code = l.Article?.CodeArticle ?? "N/A",
                    designation = l.Article?.NomArticle ?? "Inconnu",
                    dotation = l.QuantiteDotation,
                    complement = l.QuantiteCompl,
                    offerte = l.QuantiteOfferte,
                    prixPromo = l.PrixUnitairePromoEUR,
                    total = l.QuantiteOfferte * l.PrixUnitairePromoEUR
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsVenteFRS(int id)
        {
            var etat = await _db.EtatsDesVentesFRS
                .Include(e => e.EtatDesVentes)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            return Json(new
            {
                numeroEtat = etat.NumeroEtat,
                dateReception = etat.DateReception.ToString("dd/MM/yyyy"),
                flPnc = etat.EtatDesVentes?.NumeroFeuilleLigne ?? "N/A",
                totalEur = etat.ChiffreAffairesEUR,
                statut = etat.StatutControle,
                lignes = etat.Lignes.Select(l => new
                {
                    code = l.CodeArticle,
                    designation = l.NomArticle,
                    vendue = l.QuantiteVendueFRS,
                    prixUnit = l.PrixUnitaireFRS,
                    valeur = l.ValeurFRS
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsOffreFRS(int id)
        {
            var etat = await _db.EtatsDesOffresFRS
                .Include(e => e.EtatDesOffres)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            return Json(new
            {
                numeroEtat = etat.NumeroEtat,
                dateReception = etat.DateReception.ToString("dd/MM/yyyy"),
                flPnc = etat.EtatDesOffres?.NumeroFeuilleLigne ?? "N/A",
                statut = etat.StatutControle,
                lignes = etat.Lignes.Select(l => new
                {
                    code = l.CodeArticle,
                    designation = l.NomArticle,
                    dotation = l.DotationInitialeFRS,
                    restante = l.QuantiteRestanteFRS,
                    consommee = l.QuantiteConsommeeFRS
                })
            });
        }

        // ── Exportations CSV ──────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExporterVenteCSV(int id)
        {
            var etat = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"État des Ventes;FL: {etat.NumeroFeuilleLigne};Date: {etat.DateVol:dd/MM/yyyy};PNC: {(etat.PNCVendeur != null ? $"{etat.PNCVendeur.name} {etat.PNCVendeur.First_name}" : "-")}");
            sb.AppendLine("Code Article;Désignation;Dotation;Complément;Qté Vendue;Prix Unit (EUR);Total (EUR)");

            foreach (var l in etat.Lignes)
                sb.AppendLine($"{l.Article?.CodeArticle};{l.Article?.NomArticle};{l.QuantiteDotation};{l.QuantiteCompl};{l.QuantiteVendue};{l.PrixUnitaireEUR:F2};{l.QuantiteVendue * l.PrixUnitaireEUR:F2}");

            sb.AppendLine($";;;;;TOTAL (EUR):;{etat.ChiffreAffairesEUR:F2}");
            return BuildCsvResult(sb, $"Ventes_FL_{etat.NumeroFeuilleLigne}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExporterOffreCSV(int id)
        {
            var etat = await _db.EtatsDesOffres
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"État des Offres;FL: {etat.NumeroFeuilleLigne};Date: {etat.DateVol:dd/MM/yyyy}");
            sb.AppendLine("Code Article;Désignation;Dotation;Complément;Qté Offerte;Prix Promo (EUR);Total Promo (EUR)");

            foreach (var l in etat.Lignes)
                sb.AppendLine($"{l.Article?.CodeArticle};{l.Article?.NomArticle};{l.QuantiteDotation};{l.QuantiteCompl};{l.QuantiteOfferte};{l.PrixUnitairePromoEUR:F2};{l.QuantiteOfferte * l.PrixUnitairePromoEUR:F2}");

            sb.AppendLine($";;;;;VALEUR TOTALE (EUR):;{etat.ChiffreAffairesEUR:F2}");
            return BuildCsvResult(sb, $"Offres_FL_{etat.NumeroFeuilleLigne}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExporterVenteFRSCSSV(int id)
        {
            var etat = await _db.EtatsDesVentesFRS
                .Include(e => e.EtatDesVentes)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"État Ventes FRS;N° État: {etat.NumeroEtat};Date Réception: {etat.DateReception:dd/MM/yyyy};FL PNC: {etat.EtatDesVentes?.NumeroFeuilleLigne}");
            sb.AppendLine("Code Article;Désignation;Qté Vendue FRS;Prix Unit FRS (EUR);Valeur FRS (EUR)");

            foreach (var l in etat.Lignes)
                sb.AppendLine($"{l.CodeArticle};{l.NomArticle};{l.QuantiteVendueFRS};{l.PrixUnitaireFRS:F2};{l.ValeurFRS:F2}");

            sb.AppendLine($";;;TOTAL FRS (EUR):;{etat.ChiffreAffairesEUR:F2}");
            return BuildCsvResult(sb, $"Ventes_FRS_{etat.NumeroEtat}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExporterOffreFRSCSSV(int id)
        {
            var etat = await _db.EtatsDesOffresFRS
                .Include(e => e.EtatDesOffres)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"État Offres FRS;N° État: {etat.NumeroEtat};Date Réception: {etat.DateReception:dd/MM/yyyy};FL PNC: {etat.EtatDesOffres?.NumeroFeuilleLigne}");
            sb.AppendLine("Code Article;Désignation;Dotation FRS;Qté Restante FRS;Qté Consommée FRS");

            foreach (var l in etat.Lignes)
                sb.AppendLine($"{l.CodeArticle};{l.NomArticle};{l.DotationInitialeFRS};{l.QuantiteRestanteFRS};{l.QuantiteConsommeeFRS}");

            return BuildCsvResult(sb, $"Offres_FRS_{etat.NumeroEtat}.csv");
        }
    }
}
