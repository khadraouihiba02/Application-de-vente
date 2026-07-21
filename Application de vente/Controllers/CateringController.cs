using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using ApplicationDeVente.Models.ViewModels;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Catering")]
    public class CateringController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CateringController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Direction Catering";

            var debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            ViewBag.EtatsAValider = await _db.EtatsDesVentes.CountAsync(e => e.Statut == "Saisi");
            ViewBag.TrousDeCaisse = await _db.EtatsDesVentes.CountAsync(e => e.MontantEncaisseTND < e.ChiffreAffairesEUR * e.TauxChangeApplique);
            ViewBag.CATotalMois = await _db.EtatsDesVentes
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .SumAsync(e => (decimal?)e.ChiffreAffairesEUR) ?? 0;

            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // ── Contrôle Agent / FRS ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Controle(string? filtre = null, string? search = null)
        {
            ViewData["Title"] = "Contrôle Agent / FRS";
            ViewBag.Filtre = filtre ?? "tous";
            ViewBag.Search = search ?? "";

            // Récupérer tous les états de ventes PNC avec leur FRS associé (si existant)
            var queryVentes = _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes).ThenInclude(l => l.Article)
                .AsQueryable();

            // Filtrage par statut
            if (filtre == "sansifrs")
                queryVentes = queryVentes.Where(e => !_db.EtatsDesVentesFRS.Any(f => f.EtatDesVentesId == e.Id));
            else if (filtre == "avecfrs")
                queryVentes = queryVentes.Where(e => _db.EtatsDesVentesFRS.Any(f => f.EtatDesVentesId == e.Id));
            else if (filtre == "ecart")
                queryVentes = queryVentes.Where(e => _db.EtatsDesVentesFRS.Any(f => f.EtatDesVentesId == e.Id && f.StatutControle == "Écart"));
            else if (filtre == "conforme")
                queryVentes = queryVentes.Where(e => _db.EtatsDesVentesFRS.Any(f => f.EtatDesVentesId == e.Id && f.StatutControle == "Conforme"));

            if (!string.IsNullOrEmpty(search))
                queryVentes = queryVentes.Where(e => e.NumeroFeuilleLigne.Contains(search));

            var etatsVentes = await queryVentes.OrderByDescending(e => e.Id).ToListAsync();

            // Récupérer tous les FRS associés en une seule requête
            var ventesIds = etatsVentes.Select(e => e.Id).ToList();
            var etatsFRS = await _db.EtatsDesVentesFRS
                .Include(f => f.Lignes)
                .Where(f => ventesIds.Contains(f.EtatDesVentesId))
                .ToDictionaryAsync(f => f.EtatDesVentesId);

            var vm = new ControleAgentFRSViewModel
            {
                EtatsVentes = etatsVentes,
                EtatsFRS = etatsFRS
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ValiderControle(int frsId, string statut, string? commentaire)
        {
            var frs = await _db.EtatsDesVentesFRS.FindAsync(frsId);
            if (frs != null)
            {
                frs.StatutControle = statut;
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Le statut de l'état FRS a été mis à jour : {statut}.";
            }
            return RedirectToAction(nameof(Controle));
        }

        // ── Récapitulatif Mensuel ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Recapitulatif(int? mois = null, int? annee = null)
        {
            ViewData["Title"] = "Récapitulatif mensuel — Catering";

            int targetMois = mois ?? DateTime.Today.Month;
            int targetAnnee = annee ?? DateTime.Today.Year;

            var debutMois = new DateTime(targetAnnee, targetMois, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var vm = new RecapitulatifMensuelViewModel
            {
                Mois = targetMois,
                Annee = targetAnnee,
                NomMois = debutMois.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
            };

            // Charger les ventes du mois
            var etatsVentes = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .OrderByDescending(e => e.DateVol)
                .ToListAsync();

            var ventesIds = etatsVentes.Select(e => e.Id).ToList();
            var etatsFRS = await _db.EtatsDesVentesFRS
                .Where(f => ventesIds.Contains(f.EtatDesVentesId))
                .ToDictionaryAsync(f => f.EtatDesVentesId);

            // Charger les offres du mois
            var etatsOffres = await _db.EtatsDesOffres
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Include(e => e.Lignes)
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .OrderByDescending(e => e.DateVol)
                .ToListAsync();

            var offresIds = etatsOffres.Select(e => e.Id).ToList();
            var offresFRS = await _db.EtatsDesOffresFRS
                .Include(f => f.Lignes)
                .Where(f => offresIds.Contains(f.EtatDesOffresId))
                .ToDictionaryAsync(f => f.EtatDesOffresId);

            // Remplir les détails des ventes
            foreach (var e in etatsVentes)
            {
                var hasFrs = etatsFRS.ContainsKey(e.Id);
                var frs = hasFrs ? etatsFRS[e.Id] : null;
                decimal? ecart = hasFrs ? (frs!.MontantFRS - e.ChiffreAffairesEUR) : null;

                vm.VentesDetails.Add(new ItemRecapVente
                {
                    EtatId = e.Id,
                    NumeroFeuilleLigne = e.NumeroFeuilleLigne,
                    DateVol = e.DateVol,
                    VolInfo = string.Join(" / ", e.VolsList.Select(v => v.Vol?.FN_NUMBER)),
                    PNCVendeurNom = e.PNCVendeur != null ? $"{e.PNCVendeur.name} {e.PNCVendeur.First_name}" : "-",
                    MontantEURPNC = e.ChiffreAffairesEUR,
                    MontantEncaisseTND = e.MontantEncaisseTND,
                    MontantEURFRS = frs?.MontantFRS,
                    EcartEUR = ecart,
                    StatutPNC = e.Statut,
                    StatutControle = hasFrs ? frs!.StatutControle : "Non saisi"
                });
            }

            // Remplir les détails des offres
            foreach (var o in etatsOffres)
            {
                var hasFrs = offresFRS.ContainsKey(o.Id);
                var frs = hasFrs ? offresFRS[o.Id] : null;

                vm.OffresDetails.Add(new ItemRecapOffre
                {
                    EtatId = o.Id,
                    NumeroFeuilleLigne = o.NumeroFeuilleLigne,
                    DateVol = o.DateVol,
                    VolInfo = string.Join(" / ", o.VolsList.Select(v => v.Vol?.FN_NUMBER)),
                    TotalDotationPNC = o.Lignes.Sum(l => l.QuantiteDotation + l.QuantiteCompl),
                    TotalConsommePNC = o.Lignes.Sum(l => l.QuantiteOfferte),
                    TotalConsommeFRS = frs != null ? frs.Lignes.Sum(l => l.QuantiteConsommeeFRS) : 0,
                    ValeurTotaleEUR = o.ChiffreAffairesEUR,
                    StatutControle = hasFrs ? frs!.StatutControle : "Non saisi"
                });
            }

            // Calculs Globaux
            vm.TotalVentesEUR = etatsVentes.Sum(e => e.ChiffreAffairesEUR);
            vm.TotalVentesTND = etatsVentes.Sum(e => e.MontantEncaisseTND);
            vm.TotalFRSEUR = etatsFRS.Values.Sum(f => f.MontantFRS);
            vm.TotalEcartEUR = vm.VentesDetails.Where(v => v.EcartEUR.HasValue).Sum(v => v.EcartEUR!.Value);

            vm.NombreVols = etatsVentes.SelectMany(e => e.VolsList).Select(v => v.VolId).Distinct().Count();
            vm.NombreEtatsVentes = etatsVentes.Count;
            vm.NombreEtatsOffres = etatsOffres.Count;
            vm.NombreControlesConformes = etatsFRS.Values.Count(f => f.StatutControle == "Conforme");
            vm.NombreControlesEcarts = etatsFRS.Values.Count(f => f.StatutControle == "Écart");
            vm.NombreEnAttenteFRS = etatsVentes.Count - etatsFRS.Count;

            // Calcul Redevance Mensuelle
            vm.TotalPassagersEstimes = vm.NombreVols > 0 ? vm.NombreVols * 150 : 0;
            vm.RedevanceMethode1 = vm.TotalPassagersEstimes * 1.10m; // 1,10 € / passager
            vm.RedevanceMethode2 = vm.TotalVentesEUR * 0.42m;        // 42% CA Réel

            if (vm.RedevanceMethode1 >= vm.RedevanceMethode2)
            {
                vm.RedevanceRetenue = vm.RedevanceMethode1;
                vm.MethodeRetenueNom = "Méthode 1 — Minimum Garanti (1.10 € / Passager)";
            }
            else
            {
                vm.RedevanceRetenue = vm.RedevanceMethode2;
                vm.MethodeRetenueNom = "Méthode 2 — Pourcentage CA (42% × CA Réel)";
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExporterRecapitulatifCSV(int? mois = null, int? annee = null)
        {
            int targetMois = mois ?? DateTime.Today.Month;
            int targetAnnee = annee ?? DateTime.Today.Year;

            var debutMois = new DateTime(targetAnnee, targetMois, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var etatsVentes = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Include(e => e.VolsList).ThenInclude(ev => ev.Vol)
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .OrderByDescending(e => e.DateVol)
                .ToListAsync();

            var ventesIds = etatsVentes.Select(e => e.Id).ToList();
            var etatsFRS = await _db.EtatsDesVentesFRS
                .Where(f => ventesIds.Contains(f.EtatDesVentesId))
                .ToDictionaryAsync(f => f.EtatDesVentesId);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("FL;Date Vol;Vol;PNC Vendeur;CA PNC (EUR);Montant Encaissé (TND);Montant FRS (EUR);Écart (EUR);Statut FRS");

            foreach (var e in etatsVentes)
            {
                var hasFrs = etatsFRS.ContainsKey(e.Id);
                var frs = hasFrs ? etatsFRS[e.Id] : null;
                var ecart = hasFrs ? (frs!.MontantFRS - e.ChiffreAffairesEUR) : (decimal?)null;
                var volStr = string.Join("/", e.VolsList.Select(v => v.Vol?.FN_NUMBER));
                var pncStr = e.PNCVendeur != null ? $"{e.PNCVendeur.name} {e.PNCVendeur.First_name}" : "-";

                builder.AppendLine($"{e.NumeroFeuilleLigne};{e.DateVol:dd/MM/yyyy};{volStr};{pncStr};{e.ChiffreAffairesEUR:F2};{e.MontantEncaisseTND:F3};{(frs != null ? frs.MontantFRS.ToString("F2") : "N/A")};{(ecart.HasValue ? ecart.Value.ToString("F2") : "N/A")};{(hasFrs ? frs!.StatutControle : "Non saisi")}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            return File(bytes, "text/csv", $"Recapitulatif_Catering_{targetMois:D2}_{targetAnnee}.csv");
        }

        // ── Commission PNC ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Commission(int? mois = null, int? annee = null)
        {
            ViewData["Title"] = "Commission PNC";

            int targetMois = mois ?? DateTime.Today.Month;
            int targetAnnee = annee ?? DateTime.Today.Year;

            var debutMois = new DateTime(targetAnnee, targetMois, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var vm = new CommissionViewModel
            {
                Mois = targetMois,
                Annee = targetAnnee,
                NomMois = debutMois.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
            };

            var etatsVentes = await _db.EtatsDesVentes
                .Include(e => e.PNCVendeur)
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois && e.PNCVendeur != null)
                .ToListAsync();

            var ventesParPNC = etatsVentes.GroupBy(e => e.PNCVendeurId);

            foreach (var group in ventesParPNC)
            {
                var pnc = group.First().PNCVendeur;
                if (pnc == null) continue;

                var totalEncaisse = group.Sum(e => e.MontantEncaisseReel);
                var commission = totalEncaisse * 0.15m; // 15% du montant encaissé

                vm.Commissions.Add(new ItemCommission
                {
                    PNCId = pnc.Id,
                    Matricule = pnc.TLC,
                    NomComplet = $"{pnc.name} {pnc.First_name}",
                    TotalEncaisse = totalEncaisse,
                    CommissionCalculee = commission
                });
            }

            vm.TotalCommission = vm.Commissions.Sum(c => c.CommissionCalculee);

            return View(vm);
        }

        // ── Redevance mensuelle ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Redevance(int? mois = null, int? annee = null)
        {
            ViewData["Title"] = "Redevance mensuelle";

            int targetMois = mois ?? DateTime.Today.Month;
            int targetAnnee = annee ?? DateTime.Today.Year;

            // Vérifier si elle est déjà enregistrée
            var redevanceExistante = await _db.Redevances
                .FirstOrDefaultAsync(r => r.Mois == targetMois && r.Annee == targetAnnee);

            if (redevanceExistante != null)
            {
                return View(redevanceExistante);
            }

            // Sinon on la calcule
            var debutMois = new DateTime(targetAnnee, targetMois, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var etatsVentes = await _db.EtatsDesVentes
                .Include(e => e.VolsList)
                .Where(e => e.DateVol >= debutMois && e.DateVol <= finMois)
                .ToListAsync();

            int nbVols = etatsVentes.SelectMany(e => e.VolsList).Select(v => v.VolId).Distinct().Count();
            int nbPassagers = nbVols * 150; // Estimation à affiner avec les vraies données
            decimal chiffreAffaires = etatsVentes.Sum(e => e.ChiffreAffairesEUR);

            decimal montantMinGaranti = nbPassagers * 1.10m;
            decimal montantPourcentage = chiffreAffaires * 0.42m;
            
            decimal montantRetenu = montantMinGaranti >= montantPourcentage ? montantMinGaranti : montantPourcentage;
            string methode = montantMinGaranti >= montantPourcentage ? "Minimum Garanti (1.10 EUR/Pax)" : "42% du CA";

            var nouvelleRedevance = new RedevanceMensuelle
            {
                Mois = targetMois,
                Annee = targetAnnee,
                NombrePassagers = nbPassagers,
                ChiffreAffairesTotal = chiffreAffaires,
                MontantMinGaranti = montantMinGaranti,
                MontantPourcentage = montantPourcentage,
                MontantRetenu = montantRetenu,
                MethodeAppliquee = methode
            };

            return View(nouvelleRedevance);
        }

        [HttpPost]
        public async Task<IActionResult> SauvegarderRedevance(RedevanceMensuelle redevance)
        {
            redevance.Id = 0; // Sécurité pour forcer l'insertion
            redevance.DateCalcul = DateTime.Now;
            redevance.StatutFacturation = "Non facturée";

            _db.Redevances.Add(redevance);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"La redevance pour le mois {redevance.Mois}/{redevance.Annee} a été enregistrée avec succès.";
            return RedirectToAction(nameof(Redevance), new { mois = redevance.Mois, annee = redevance.Annee });
        }

        // ── Valider Factures ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ValiderFactures()
        {
            ViewData["Title"] = "Valider factures";
            
            var vm = new ValiderFacturesViewModel
            {
                FacturesEnAttente = await _db.Factures.Where(f => f.Statut == "En attente").OrderBy(f => f.DateFacture).ToListAsync(),
                HistoriqueFactures = await _db.Factures.Where(f => f.Statut != "En attente").OrderByDescending(f => f.DateFacture).Take(20).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ChangerStatutFacture(int id, string statut)
        {
            var facture = await _db.Factures.FindAsync(id);
            if (facture != null)
            {
                facture.Statut = statut;
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"La facture N° {facture.NumeroFacture} a été marquée comme : {statut}.";
            }
            return RedirectToAction(nameof(ValiderFactures));
        }

        [HttpPost]
        public async Task<IActionResult> SimulerAjoutFacture(string NumeroFacture, string Type, decimal Montant)
        {
            var nouvelleFacture = new Facture
            {
                NumeroFacture = NumeroFacture,
                DateFacture = DateTime.Today,
                Type = Type,
                Montant = Montant,
                Statut = "En attente"
            };

            _db.Factures.Add(nouvelleFacture);
            await _db.SaveChangesAsync();

            TempData["Succes"] = $"La facture N° {NumeroFacture} a été ajoutée avec succès (Simulation).";
            return RedirectToAction(nameof(ValiderFactures));
        }

        [HttpPost]
        public async Task<IActionResult> ImporterFactures(IFormFile fichierCsv)
        {
            if (fichierCsv == null || fichierCsv.Length == 0)
            {
                TempData["Erreur"] = "Veuillez sélectionner un fichier valide.";
                return RedirectToAction(nameof(ValiderFactures));
            }

            int count = 0;
            using (var stream = new StreamReader(fichierCsv.OpenReadStream()))
            {
                // Ignorer l'entête (si on suppose qu'il y en a un)
                string? header = await stream.ReadLineAsync();
                
                while (!stream.EndOfStream)
                {
                    string? line = await stream.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length >= 3)
                    {
                        // Remplacer les points par des virgules ou inversement selon la culture si besoin, 
                        // ici on force Culture Invariant (qui attend un point pour les décimales : 150.50)
                        if (decimal.TryParse(values[2].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal montant))
                        {
                            var facture = new Facture
                            {
                                NumeroFacture = values[0].Trim(),
                                Type = values[1].Trim(),
                                Montant = montant,
                                DateFacture = DateTime.Today,
                                Statut = "En attente"
                            };
                            _db.Factures.Add(facture);
                            count++;
                        }
                    }
                }
            }
            
            if (count > 0)
            {
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"{count} facture(s) importée(s) avec succès !";
            }
            else
            {
                TempData["Erreur"] = "Aucune facture valide n'a pu être lue depuis le fichier. Vérifiez le format (NumeroFacture,Type,Montant).";
            }

            return RedirectToAction(nameof(ValiderFactures));
        }
    }
}
