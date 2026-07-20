using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Models.ViewModels;
namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Admin,Catering")]
    public class ParametrageVolController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ParametrageVolController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Vols";
            var vols = await _db.Vols.OrderBy(v => v.NumeroVol).ToListAsync();
            return View(vols);
        }

        public IActionResult Creer()
        {
            ViewData["Title"] = "Nouveau Vol";
            return View(new Vol());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Vol model)
        {
            if (_db.Vols.Any(v => v.NumeroVol == model.NumeroVol && v.DateVol == model.DateVol))
                ModelState.AddModelError("NumeroVol", "Ce numéro de vol existe déjà pour cette date.");

            if (ModelState.IsValid)
            {
                _db.Vols.Add(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {model.NumeroVol} ({model.Origine}/{model.Destination}) ajouté avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Nouveau Vol";
            return View(model);
        }

        public async Task<IActionResult> Modifier(int id)
        {
            ViewData["Title"] = "Modifier Vol";
            var vol = await _db.Vols.FindAsync(id);
            if (vol == null) return NotFound();
            return View(vol);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Vol model)
        {
            if (_db.Vols.Any(v => v.NumeroVol == model.NumeroVol && v.DateVol == model.DateVol && v.Id != model.Id))
                ModelState.AddModelError("NumeroVol", "Ce numéro de vol est déjà utilisé pour cette date.");

            if (ModelState.IsValid)
            {
                _db.Vols.Update(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {model.NumeroVol} modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Modifier Vol";
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActif(int id)
        {
            var vol = await _db.Vols.FindAsync(id);
            if (vol != null)
            {
                vol.Actif = !vol.Actif;
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {vol.NumeroVol} {(vol.Actif ? "activé" : "désactivé")}.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Supprimer(int id)
        {
            var vol = await _db.Vols.FindAsync(id);
            if (vol != null)
            {
                _db.Vols.Remove(vol);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {vol.NumeroVol} a été supprimé.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActionEnMasse(int[] ids, string actionType)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Erreur"] = "Aucun vol sélectionné.";
                return RedirectToAction(nameof(Index));
            }

            var vols = await _db.Vols.Where(v => ids.Contains(v.Id)).ToListAsync();
            
            if (actionType == "Supprimer")
            {
                _db.Vols.RemoveRange(vols);
                TempData["Succes"] = $"{vols.Count} vols supprimés avec succès.";
            }
            else if (actionType == "Activer")
            {
                vols.ForEach(v => v.Actif = true);
                TempData["Succes"] = $"{vols.Count} vols activés avec succès.";
            }
            else if (actionType == "Desactiver")
            {
                vols.ForEach(v => v.Actif = false);
                TempData["Succes"] = $"{vols.Count} vols désactivés avec succès.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── Import / Export Excel ─────────────────────────────────────
        public IActionResult TelechargerModele()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Modèle Vols");
            
            worksheet.Cell(1, 1).Value = "Numéro Vol";
            worksheet.Cell(1, 2).Value = "Origine";
            worksheet.Cell(1, 3).Value = "Destination";
            
            worksheet.Range("A1:C1").Style.Font.Bold = true;
            worksheet.Range("A1:C1").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            worksheet.Columns().AdjustToContents();

            worksheet.Cell(2, 1).Value = "TU202";
            worksheet.Cell(2, 2).Value = "TUN";
            worksheet.Cell(2, 3).Value = "CDG";

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Modele_Import_Vols.xlsx");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ImporterExcel(IFormFile fichierExcel)
        {
            if (fichierExcel == null || fichierExcel.Length == 0)
            {
                TempData["Erreur"] = "Fichier Excel invalide.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new System.IO.MemoryStream();
                await fichierExcel.CopyToAsync(stream);
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var rows = workbook.Worksheet(1).RangeUsed().RowsUsed().Skip(1);

                int ajoutes = 0;
                int ignores = 0;

                foreach (var row in rows)
                {
                    string numVol = row.Cell(1).GetValue<string>().Trim().ToUpper();
                    string origine = row.Cell(2).GetValue<string>().Trim().ToUpper();
                    string dest = row.Cell(3).GetValue<string>().Trim().ToUpper();

                    if (!string.IsNullOrEmpty(numVol))
                    {
                        if (!_db.Vols.Any(v => v.NumeroVol == numVol))
                        {
                            _db.Vols.Add(new Vol { NumeroVol = numVol, Origine = origine, Destination = dest, Actif = true });
                            ajoutes++;
                        }
                        else
                        {
                            ignores++;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Importation : {ajoutes} vols ajoutés, {ignores} ignorés.";
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur d'importation : " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
