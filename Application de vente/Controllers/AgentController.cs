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

            // Charger les 5 dernières saisies de ventes FRS
            vm.DerniersEtatsFRS = await _db.EtatsDesVentesFRS
                .Include(e => e.EtatDesVentes).ThenInclude(ev => ev.VolsList).ThenInclude(ev => ev.Vol)
                .OrderByDescending(e => e.Id)
                .Take(5)
                .ToListAsync();

            // Charger les 5 dernières saisies d'offres FRS
            vm.DerniersEtatsOffresFRS = await _db.EtatsDesOffresFRS
                .Include(e => e.EtatDesOffres).ThenInclude(ev => ev.VolsList).ThenInclude(ev => ev.Vol)
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

            // Tous les vols actifs disponibles
            vm.VolsDisponibles = await _db.Vols
                .Where(v => v.Actif)
                .Select(v => new SelectListItem 
                { 
                    Value = v.Id.ToString(), 
                    Text = $"{v.FN_NUMBER} ({v.DEP_AP_ACTUAL} - {v.ARR_AP_ACTUAL})" 
                })
                .ToListAsync();

            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            vm.TauxChangeApplique = tauxActif?.Taux ?? 3.4000m;

            // On n'a plus besoin de charger TousPNCs ici car c'est géré via AJAX maintenant.
            vm.TousPNCs = new List<SelectListItem>();

            // La grille des articles est vide au départ
            vm.LignesArticles = new List<LigneSaisieArticle>();
            vm.PNCsDisponibles = new List<SelectListItem>();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirVentes(SaisieVentesViewModel vm)
        {
            if (!ModelState.IsValid || vm.VolId == 0)
            {
                vm.VolsDisponibles = await _db.Vols.Where(v => v.Actif).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.FN_NUMBER} ({v.DEP_AP_ACTUAL} - {v.ARR_AP_ACTUAL}) - {v.DAY_OF_ORIGIN:dd/MM/yyyy}" }).ToListAsync();
                if(vm.LignesArticles == null) vm.LignesArticles = new List<LigneSaisieArticle>();
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
                MontantEncaisseReel = vm.MontantEncaisseReel,
                Statut = "Saisi"
            };

            // Lier le vol sélectionné
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

            // Gérer les offres si des lignes ont été saisies
            if (vm.LignesOffres != null && vm.LignesOffres.Any(l => l.ArticleId > 0))
            {
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
                _db.EtatsDesOffres.Add(etatOffres);
            }

            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des ventes (et offres si saisies) pour la FL {vm.NumeroFeuilleLigne} a été enregistré avec succès.";
            return RedirectToAction(nameof(Dashboard));
        }



        // ── Saisie des Ventes FRS ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SaisirVentesFRS()
        {
            var vm = new SaisieVentesFRSViewModel();
            var aujourdhui = DateTime.Today;
            
            var dejaSaisis = await _db.EtatsDesVentesFRS.Select(f => f.EtatDesVentesId).ToListAsync();
            
            vm.EtatsPNCDisponibles = await _db.EtatsDesVentes
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Where(e => !dejaSaisis.Contains(e.Id))
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"FL: {e.NumeroFeuilleLigne} | Vol: {(e.VolsList.FirstOrDefault() != null ? e.VolsList.First().Vol.FN_NUMBER : "N/A")} | Date: {e.DateVol.ToString("dd/MM/yyyy")}"
                })
                .ToListAsync();

            var tauxActif = await _db.TauxChanges
                .Where(t => t.DeviseCible == "TND" && aujourdhui >= t.DateDebut && aujourdhui <= t.DateFin)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            vm.TauxChangeApplique = tauxActif?.Taux ?? 3.4000m;

            vm.LignesArticles = new List<LigneSaisieVenteFRS>();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaisirVentesFRS(SaisieVentesFRSViewModel vm)
        {
            if (!ModelState.IsValid || vm.EtatDesVentesId == 0)
            {
                var dejaSaisis = await _db.EtatsDesVentesFRS.Select(f => f.EtatDesVentesId).ToListAsync();
                vm.EtatsPNCDisponibles = await _db.EtatsDesVentes
                    .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                    .Where(e => !dejaSaisis.Contains(e.Id))
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = $"FL: {e.NumeroFeuilleLigne} | Vol: {(e.VolsList.FirstOrDefault() != null ? e.VolsList.First().Vol.FN_NUMBER : "N/A")} | Date: {e.DateVol.ToString("dd/MM/yyyy")}"
                    })
                    .ToListAsync();

                if (vm.LignesArticles == null) vm.LignesArticles = new List<LigneSaisieVenteFRS>();
                return View(vm);
            }

            var lignesFiltrees = vm.LignesArticles.Where(l => l.QuantiteVendueFRS > 0).ToList();
            decimal totalEur = lignesFiltrees.Sum(l => l.QuantiteVendueFRS * l.PrixUnitaireFRS);
            decimal totalTnd = totalEur * vm.TauxChangeApplique;

            var etatVentesFRS = new EtatDesVentesFRS
            {
                NumeroEtat = vm.NumeroEtat,
                DateReception = vm.DateReception,
                EtatDesVentesId = vm.EtatDesVentesId,
                MontantFRS = 0, // Optionnel, mais on utilise le nouveau format TND
                TauxChangeApplique = vm.TauxChangeApplique,
                ChiffreAffairesEUR = totalEur,
                MontantTheoriqueTND = totalTnd,
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

            // Gérer les offres FRS si des lignes ont été saisies
            if (vm.LignesOffres != null && vm.LignesOffres.Any(l => l.DotationInitialeFRS > 0 || l.QuantiteRestanteFRS > 0))
            {
                // Trouver l'ID de l'état des offres PNC associé s'il existe
                var etatVentesPNC = await _db.EtatsDesVentes.FindAsync(vm.EtatDesVentesId);
                var etatOffresPNC = await _db.EtatsDesOffres.FirstOrDefaultAsync(o => o.NumeroFeuilleLigne == etatVentesPNC.NumeroFeuilleLigne && o.DateVol == etatVentesPNC.DateVol);

                if (etatOffresPNC != null)
                {
                    var lignesOffresFiltrees = vm.LignesOffres.Where(l => l.DotationInitialeFRS > 0 || l.QuantiteRestanteFRS > 0).ToList();
                    var etatOffresFRS = new EtatDesOffresFRS
                    {
                        NumeroEtat = vm.NumeroEtat,
                        DateReception = vm.DateReception,
                        EtatDesOffresId = etatOffresPNC.Id,
                        StatutControle = "En attente"
                    };

                    foreach (var ligne in lignesOffresFiltrees)
                    {
                        int qteConsommee = ligne.DotationInitialeFRS - ligne.QuantiteRestanteFRS;
                        if (qteConsommee < 0) qteConsommee = 0;

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
            }

            await _db.SaveChangesAsync();

            TempData["Succes"] = $"L'état des ventes FRS N° {vm.NumeroEtat} a été enregistré avec succès.";
            return RedirectToAction(nameof(Dashboard));
        }



        // ── Endpoints API pour l'interface dynamique ─────────────────

        [HttpGet]
        public async Task<IActionResult> GetCrewsByVol(int volId)
        {
            if (volId == 0)
                return Json(new List<object>());

            var vol = await _db.Vols.FindAsync(volId);
            if (vol == null)
                return Json(new List<object>());
            
            // On cherche les crews où le FlightNumber et Day_of_origin correspondent au vol sélectionné
            var crews = await _db.PNCs
                .Where(p => p.FlightNumber == vol.FN_NUMBER && p.Day_of_origin.Date == vol.DAY_OF_ORIGIN.Date)
                .Select(p => new { 
                    id = p.Id, 
                    texte = $"{p.TLC} - {p.name} {p.First_name} ({p.Rank})" 
                })
                .ToListAsync();

            return Json(crews);
        }

        [HttpGet]
        public async Task<IActionResult> GetVolsByDate(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                if (!DateTime.TryParse(date, out parsedDate))
                {
                    return Json(new List<object>());
                }
            }

            var vols = await _db.Vols
                .Where(v => v.DAY_OF_ORIGIN.Date == parsedDate.Date)
                .Select(v => new {
                    id = v.Id,
                    texte = $"{v.FN_NUMBER} ({v.DEP_AP_ACTUAL} - {v.ARR_AP_ACTUAL}) - {v.DAY_OF_ORIGIN.ToString("dd/MM/yyyy")}"
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
                .Select(a => new {
                    id = a.Id,
                    code = a.CodeArticle,
                    designation = a.NomArticle,
                    prix = a.PrixUnitaire
                })
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

            var result = new
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
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsOffre(int id)
        {
            var etat = await _db.EtatsDesOffres
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var result = new
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
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsVenteFRS(int id)
        {
            var etat = await _db.EtatsDesVentesFRS
                .Include(e => e.EtatDesVentes)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var result = new
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
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsOffreFRS(int id)
        {
            var etat = await _db.EtatsDesOffresFRS
                .Include(e => e.EtatDesOffres)
                .Include(e => e.Lignes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat == null) return NotFound();

            var result = new
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
            };

            return Json(result);
        }

        // Exportations CSV
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
            {
                sb.AppendLine($"{l.Article?.CodeArticle};{l.Article?.NomArticle};{l.QuantiteDotation};{l.QuantiteCompl};{l.QuantiteVendue};{l.PrixUnitaireEUR:F2};{(l.QuantiteVendue * l.PrixUnitaireEUR):F2}");
            }
            sb.AppendLine($";;;;;TOTAL (EUR):;{etat.ChiffreAffairesEUR:F2}");

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Ventes_FL_{etat.NumeroFeuilleLigne}.csv");
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
            {
                sb.AppendLine($"{l.Article?.CodeArticle};{l.Article?.NomArticle};{l.QuantiteDotation};{l.QuantiteCompl};{l.QuantiteOfferte};{l.PrixUnitairePromoEUR:F2};{(l.QuantiteOfferte * l.PrixUnitairePromoEUR):F2}");
            }
            sb.AppendLine($";;;;;VALEUR TOTALE (EUR):;{etat.ChiffreAffairesEUR:F2}");

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Offres_FL_{etat.NumeroFeuilleLigne}.csv");
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
            {
                sb.AppendLine($"{l.CodeArticle};{l.NomArticle};{l.QuantiteVendueFRS};{l.PrixUnitaireFRS:F2};{l.ValeurFRS:F2}");
            }
            sb.AppendLine($";;;TOTAL FRS (EUR):;{etat.ChiffreAffairesEUR:F2}");

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Ventes_FRS_{etat.NumeroEtat}.csv");
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
            {
                sb.AppendLine($"{l.CodeArticle};{l.NomArticle};{l.DotationInitialeFRS};{l.QuantiteRestanteFRS};{l.QuantiteConsommeeFRS}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Offres_FRS_{etat.NumeroEtat}.csv");
        }
    }
}
