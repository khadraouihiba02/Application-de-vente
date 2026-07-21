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

        // ── Autres pages (stubs) ─────────────────────────────────────
        public IActionResult Recapitulatif() { ViewData["Title"] = "Récapitulatif mensuel"; return View(); }
        public IActionResult Commission() { ViewData["Title"] = "Commission PNC"; return View(); }
        public IActionResult Redevance() { ViewData["Title"] = "Redevance mensuelle"; return View(); }
        public IActionResult ValiderFactures() { ViewData["Title"] = "Valider factures"; return View(); }
    }
}
