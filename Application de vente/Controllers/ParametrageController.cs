using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Catering")]
    public class ParametrageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ParametrageController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══════════════════════════════════════
        // MODULE ARTICLES
        // ═══════════════════════════════════════

        // GET: /Parametrage/Articles
        public async Task<IActionResult> Articles()
        {
            ViewData["Title"] = "Gestion des Articles";
            var articles = await _db.Articles
                                    .OrderBy(a => a.CodeArticle)
                                    .ToListAsync();
            return View(articles);
        }

        // GET: /Parametrage/CreerArticle
        public IActionResult CreerArticle()
        {
            ViewData["Title"] = "Nouvel Article";
            return View(new Article
            {
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(6)
            });
        }

        // POST: /Parametrage/CreerArticle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerArticle(Article article)
        {
            if (ModelState.IsValid)
            {
                if (article.DateFin <= article.DateDebut)
                {
                    ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
                    ViewData["Title"] = "Nouvel Article";
                    return View(article);
                }

                _db.Articles.Add(article);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"L'article '{article.NomArticle}' a été créé avec succès.";
                return RedirectToAction(nameof(Articles));
            }

            ViewData["Title"] = "Nouvel Article";
            return View(article);
        }

        // GET: /Parametrage/ModifierArticle/5
        public async Task<IActionResult> ModifierArticle(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null) return NotFound();

            ViewData["Title"] = "Modifier l'Article";
            return View(article);
        }

        // POST: /Parametrage/ModifierArticle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierArticle(int id, Article article)
        {
            if (id != article.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (article.DateFin <= article.DateDebut)
                {
                    ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
                    ViewData["Title"] = "Modifier l'Article";
                    return View(article);
                }

                _db.Articles.Update(article);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"L'article '{article.NomArticle}' a été modifié avec succès.";
                return RedirectToAction(nameof(Articles));
            }

            ViewData["Title"] = "Modifier l'Article";
            return View(article);
        }

        // POST: /Parametrage/SupprimerArticle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerArticle(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article != null)
            {
                _db.Articles.Remove(article);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"L'article '{article.NomArticle}' a été supprimé.";
            }
            return RedirectToAction(nameof(Articles));
        }

        // POST: /Parametrage/ImporterArticles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImporterArticles(IFormFile fichierExcel)
        {
            if (fichierExcel == null || fichierExcel.Length == 0)
            {
                TempData["Erreur"] = "Veuillez sélectionner un fichier valide.";
                return RedirectToAction(nameof(Articles));
            }

            if (!fichierExcel.FileName.EndsWith(".xlsx") && !fichierExcel.FileName.EndsWith(".csv"))
            {
                TempData["Erreur"] = "Seuls les fichiers Excel (.xlsx) et CSV sont supportés.";
                return RedirectToAction(nameof(Articles));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await fichierExcel.CopyToAsync(stream);
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Ignorer l'en-tête

                        int ajoutCount = 0;
                        foreach (var row in rows)
                        {
                            var article = new Article
                            {
                                CodeArticle = row.Cell(1).GetValue<string>(),
                                NomArticle = row.Cell(2).GetValue<string>(),
                                Description = row.Cell(3).GetValue<string>(),
                                PrixUnitaire = row.Cell(4).GetValue<decimal>(),
                                DateDebut = row.Cell(5).GetValue<DateTime>(),
                                DateFin = row.Cell(6).GetValue<DateTime>()
                            };

                            _db.Articles.Add(article);
                            ajoutCount++;
                        }
                        await _db.SaveChangesAsync();
                        TempData["Succes"] = $"{ajoutCount} articles ont été importés avec succès depuis le fichier.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur lors de l'importation : Vérifiez le format de votre fichier. L'ordre attendu : Code, Nom, Description, Prix, DateDebut, DateFin.";
            }

            return RedirectToAction(nameof(Articles));
        }

        // ═══════════════════════════════════════
        // MODULE TAUX DE CHANGE
        // ═══════════════════════════════════════

        // GET: /Parametrage/TauxChange
        public async Task<IActionResult> TauxChange()
        {
            ViewData["Title"] = "Gestion des Taux de Change (EUR/TND)";
            var taux = await _db.TauxChanges
                                .OrderByDescending(t => t.DateDebut)
                                .ToListAsync();
            return View(taux);
        }

        // GET: /Parametrage/CreerTaux
        public IActionResult CreerTaux()
        {
            ViewData["Title"] = "Nouveau Taux de Change";
            return View(new TauxChange
            {
                DeviseSource = "EUR",
                DeviseCible = "TND",
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(1)
            });
        }

        // POST: /Parametrage/CreerTaux
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerTaux(TauxChange model)
        {
            if (ModelState.IsValid)
            {
                if (model.DateFin <= model.DateDebut)
                {
                    ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
                    ViewData["Title"] = "Nouveau Taux de Change";
                    return View(model);
                }

                _db.TauxChanges.Add(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Taux de change ({model.Taux} {model.DeviseSource}/{model.DeviseCible}) ajouté avec succès.";
                return RedirectToAction(nameof(TauxChange));
            }

            ViewData["Title"] = "Nouveau Taux de Change";
            return View(model);
        }

        // POST: /Parametrage/SupprimerTaux/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerTaux(int id)
        {
            var taux = await _db.TauxChanges.FindAsync(id);
            if (taux != null)
            {
                _db.TauxChanges.Remove(taux);
                await _db.SaveChangesAsync();
                TempData["Succes"] = "Taux de change supprimé avec succès.";
            }
            return RedirectToAction(nameof(TauxChange));
        }
    }
}
