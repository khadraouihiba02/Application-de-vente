using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Admin,Catering")]
    public class ParametragePNCController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ParametragePNCController(ApplicationDbContext db) => _db = db;

        // ── Liste PNC ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Cabin Crews";
            var pncs = await _db.PNCs.OrderBy(p => p.Day_of_origin).ThenBy(p => p.FlightNumber).ToListAsync();
            return View(pncs);
        }

        // ── Créer PNC ─────────────────────────────────────────────────
        public IActionResult Creer()
        {
            ViewData["Title"] = "Nouveau Crew";
            return View(new PNC());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(PNC model)
        {
            if (ModelState.IsValid)
            {
                _db.PNCs.Add(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Crew {model.First_name} {model.name} ({model.TLC}) ajouté pour le vol {model.FlightNumber}.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Nouveau Crew";
            return View(model);
        }

        // ── Modifier PNC ──────────────────────────────────────────────
        public async Task<IActionResult> Modifier(int id)
        {
            ViewData["Title"] = "Modifier Crew";
            var pnc = await _db.PNCs.FindAsync(id);
            if (pnc == null) return NotFound();
            return View(pnc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(PNC model)
        {
            if (ModelState.IsValid)
            {
                _db.PNCs.Update(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Crew {model.First_name} {model.name} modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Modifier Crew";
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Supprimer(int id)
        {
            var pnc = await _db.PNCs.FindAsync(id);
            if (pnc != null)
            {
                _db.PNCs.Remove(pnc);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Crew {pnc.First_name} {pnc.name} ({pnc.TLC}) a été supprimé.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActionEnMasse(int[] ids, string actionType)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Erreur"] = "Aucun Crew sélectionné.";
                return RedirectToAction(nameof(Index));
            }

            var pncs = await _db.PNCs.Where(p => ids.Contains(p.Id)).ToListAsync();
            
            if (actionType == "Supprimer")
            {
                _db.PNCs.RemoveRange(pncs);
                TempData["Succes"] = $"{pncs.Count} Crews supprimés avec succès.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── Import / Export Excel ─────────────────────────────────────
        public IActionResult TelechargerModele()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Modèle Cabin Crew");
            
            // En-têtes correspondants à la saisie manuelle
            worksheet.Cell(1, 1).Value = "FlightNumber";
            worksheet.Cell(1, 2).Value = "Day_of_origin";
            worksheet.Cell(1, 3).Value = "departure";
            worksheet.Cell(1, 4).Value = "destination";
            worksheet.Cell(1, 5).Value = "TLC";
            worksheet.Cell(1, 6).Value = "Rank";
            worksheet.Cell(1, 7).Value = "name";
            worksheet.Cell(1, 8).Value = "First_name";
            
            // Style
            worksheet.Range("A1:H1").Style.Font.Bold = true;
            worksheet.Range("A1:H1").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            // Exemple
            worksheet.Cell(2, 1).Value = "TU202";
            worksheet.Cell(2, 2).Value = DateTime.Today.ToString("dd/MM/yyyy");
            worksheet.Cell(2, 3).Value = "TUN";
            worksheet.Cell(2, 4).Value = "CDG";
            worksheet.Cell(2, 5).Value = "BA1";
            worksheet.Cell(2, 6).Value = "PNC";
            worksheet.Cell(2, 7).Value = "BEN ALI";
            worksheet.Cell(2, 8).Value = "AHMED";

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Modele_Import_CabinCrew.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImporterExcel(IFormFile fichierExcel)
        {
            if (fichierExcel == null || fichierExcel.Length == 0)
            {
                TempData["Erreur"] = "Veuillez sélectionner un fichier Excel valide.";
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
                int colFn = 1, colDate = 2, colDep = 3, colArr = 4, colTlc = 5, colRank = 6, colNom = 7, colPrenom = 8;

                // Détection dynamique des colonnes par nom
                for (int c = 1; c <= headerRow.LastCellUsed().Address.ColumnNumber; c++)
                {
                    string h = headerRow.Cell(c).GetString().Trim().ToUpper();
                    if (h.Contains("FLIGHTNUMBER") || h.Contains("VOL") || h.Contains("FN_NUMBER")) colFn = c;
                    else if (h.Contains("DAY_OF_ORIGIN") || h.Contains("DATE")) colDate = c;
                    else if (h.Contains("DEPARTURE") || h.Contains("ORIGINE") || h.Contains("DEP")) colDep = c;
                    else if (h.Contains("DESTINATION") || h.Contains("DEST") || h.Contains("ARR")) colArr = c;
                    else if (h.Contains("TLC") || h.Contains("MATRICULE")) colTlc = c;
                    else if (h.Contains("RANK") || h.Contains("GRADE")) colRank = c;
                    else if (h == "NAME" || h.Contains("NOM")) colNom = c;
                    else if (h.Contains("FIRST_NAME") || h.Contains("PRÉNOM") || h.Contains("PRENOM")) colPrenom = c;
                }

                var rows = range.RowsUsed().Skip(1);
                int ajoutes = 0;

                foreach (var row in rows)
                {
                    string fn = GetCellString(row.Cell(colFn)).ToUpper();
                    string tlc = GetCellString(row.Cell(colTlc));
                    if (string.IsNullOrEmpty(fn) || string.IsNullOrEmpty(tlc)) continue;

                    DateTime dateOrigin = ParseCellDate(row.Cell(colDate));
                    string dep = GetCellString(row.Cell(colDep)).ToUpper();
                    string arr = GetCellString(row.Cell(colArr)).ToUpper();
                    string rank = GetCellString(row.Cell(colRank));
                    string nom = GetCellString(row.Cell(colNom));
                    string prenom = GetCellString(row.Cell(colPrenom));

                    _db.PNCs.Add(new PNC
                    {
                        FlightNumber = fn,
                        Day_of_origin = dateOrigin,
                        departure = dep,
                        destination = arr,
                        TLC = tlc,
                        Rank = rank,
                        name = nom,
                        First_name = prenom
                    });
                    ajoutes++;
                }

                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Importation terminée : {ajoutes} Cabin Crew(s) ajouté(s).";
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur lors de l'importation. Détail: " + ex.Message;
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
