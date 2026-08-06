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
            var vols = await _db.Vols.OrderBy(v => v.FN_NUMBER).ToListAsync();
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
            if (_db.Vols.Any(v => v.FN_NUMBER == model.FN_NUMBER && v.DAY_OF_ORIGIN == model.DAY_OF_ORIGIN))
                ModelState.AddModelError("FN_NUMBER", "Ce numéro de vol existe déjà pour cette date.");

            if (ModelState.IsValid)
            {
                _db.Vols.Add(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {model.FN_NUMBER} ({model.DEP_AP_ACTUAL}/{model.ARR_AP_ACTUAL}) ajouté avec succès.";
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
            if (_db.Vols.Any(v => v.FN_NUMBER == model.FN_NUMBER && v.DAY_OF_ORIGIN == model.DAY_OF_ORIGIN && v.Id != model.Id))
                ModelState.AddModelError("FN_NUMBER", "Ce numéro de vol est déjà utilisé pour cette date.");

            if (ModelState.IsValid)
            {
                _db.Vols.Update(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Vol {model.FN_NUMBER} modifié avec succès.";
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
                TempData["Succes"] = $"Vol {vol.FN_NUMBER} {(vol.Actif ? "activé" : "désactivé")}.";
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
                TempData["Succes"] = $"Vol {vol.FN_NUMBER} a été supprimé.";
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
            
            worksheet.Cell(1, 1).Value = "FN_NUMBER";
            worksheet.Cell(1, 2).Value = "DAY_OF_ORIGIN";
            worksheet.Cell(1, 3).Value = "DEP_AP_ACTUAL";
            worksheet.Cell(1, 4).Value = "ARR_AP_ACTUAL";
            worksheet.Cell(1, 5).Value = "Actif";
            
            worksheet.Range("A1:E1").Style.Font.Bold = true;
            worksheet.Range("A1:E1").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            // Ligne d'exemple
            worksheet.Cell(2, 1).Value = "TU202";
            worksheet.Cell(2, 2).Value = DateTime.Today.ToString("dd/MM/yyyy");
            worksheet.Cell(2, 3).Value = "TUN";
            worksheet.Cell(2, 4).Value = "CDG";
            worksheet.Cell(2, 5).Value = "Oui";

            worksheet.Columns().AdjustToContents();

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
                var worksheet = workbook.Worksheet(1);
                var range = worksheet.RangeUsed();
                if (range == null || range.RowsUsed().Count() <= 1)
                {
                    TempData["Erreur"] = "Le fichier Excel est vide.";
                    return RedirectToAction(nameof(Index));
                }

                var headerRow = range.FirstRow();
                int colFn = 1, colDate = 2, colDep = 3, colArr = 4, colActif = 5;

                // Détection dynamique des colonnes par nom
                for (int c = 1; c <= headerRow.LastCellUsed().Address.ColumnNumber; c++)
                {
                    string name = headerRow.Cell(c).GetString().Trim().ToUpper();
                    if (name.Contains("FN_NUMBER") || name.Contains("NUMÉRO") || name.Contains("VOL")) colFn = c;
                    else if (name.Contains("DAY_OF_ORIGIN") || name.Contains("DATE")) colDate = c;
                    else if (name.Contains("DEP_AP_ACTUAL") || name.Contains("DEP") || name.Contains("ORIGINE")) colDep = c;
                    else if (name.Contains("ARR_AP_ACTUAL") || name.Contains("ARR") || name.Contains("DEST")) colArr = c;
                    else if (name.Contains("ACTIF") || name.Contains("STATUT")) colActif = c;
                }

                var rows = range.RowsUsed().Skip(1);
                int ajoutes = 0;
                int majs = 0;

                foreach (var row in rows)
                {
                    string numVol = GetCellString(row.Cell(colFn)).ToUpper();
                    if (string.IsNullOrEmpty(numVol)) continue;

                    DateTime dateVol = ParseCellDate(row.Cell(colDate));
                    string origine = GetCellString(row.Cell(colDep)).ToUpper();
                    string dest = GetCellString(row.Cell(colArr)).ToUpper();
                    string actifStr = GetCellString(row.Cell(colActif)).ToLower();
                    bool actif = string.IsNullOrEmpty(actifStr) || actifStr == "oui" || actifStr == "true" || actifStr == "1" || actifStr == "actif";

                    var existing = await _db.Vols.FirstOrDefaultAsync(v => v.FN_NUMBER == numVol && v.DAY_OF_ORIGIN.Date == dateVol.Date);
                    if (existing != null)
                    {
                        existing.DEP_AP_ACTUAL = !string.IsNullOrEmpty(origine) ? origine : existing.DEP_AP_ACTUAL;
                        existing.ARR_AP_ACTUAL = !string.IsNullOrEmpty(dest) ? dest : existing.ARR_AP_ACTUAL;
                        existing.Actif = actif;
                        majs++;
                    }
                    else
                    {
                        _db.Vols.Add(new Vol
                        {
                            FN_NUMBER = numVol,
                            DAY_OF_ORIGIN = dateVol,
                            DEP_AP_ACTUAL = origine,
                            ARR_AP_ACTUAL = dest,
                            Actif = actif
                        });
                        ajoutes++;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Importation terminée : {ajoutes} vol(s) ajouté(s), {majs} mis à jour.";
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur d'importation : " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static string GetCellString(ClosedXML.Excel.IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return string.Empty;
            return cell.GetString().Trim();
        }

        private static DateTime ParseCellDate(ClosedXML.Excel.IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return DateTime.Today;
            if (cell.DataType == ClosedXML.Excel.XLDataType.DateTime) return cell.GetDateTime();
            if (cell.DataType == ClosedXML.Excel.XLDataType.Number)
            {
                try { return DateTime.FromOADate(cell.GetDouble()); } catch { }
            }
            string str = cell.GetString().Trim();
            if (DateTime.TryParse(str, out DateTime dt)) return dt;
            return DateTime.Today;
        }
    }
}
