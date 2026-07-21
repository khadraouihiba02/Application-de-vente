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
            
            // En-têtes
            worksheet.Cell(1, 1).Value = "Day_of_origin";
            worksheet.Cell(1, 2).Value = "FlightNumber";
            worksheet.Cell(1, 3).Value = "departure";
            worksheet.Cell(1, 4).Value = "destination";
            worksheet.Cell(1, 5).Value = "TLC";
            worksheet.Cell(1, 6).Value = "name";
            worksheet.Cell(1, 7).Value = "First_name";
            worksheet.Cell(1, 8).Value = "Rank";
            
            // Style
            worksheet.Range("A1:H1").Style.Font.Bold = true;
            worksheet.Range("A1:H1").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            worksheet.Columns().AdjustToContents();

            // Exemple
            worksheet.Cell(2, 1).Value = DateTime.Today.ToString("dd/MM/yyyy");
            worksheet.Cell(2, 2).Value = "TU202";
            worksheet.Cell(2, 3).Value = "TUN";
            worksheet.Cell(2, 4).Value = "CDG";
            worksheet.Cell(2, 5).Value = "BA1";
            worksheet.Cell(2, 6).Value = "BEN ALI";
            worksheet.Cell(2, 7).Value = "AHMED";
            worksheet.Cell(2, 8).Value = "PNC";

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
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Ignorer l'en-tête

                int ajoutes = 0;

                foreach (var row in rows)
                {
                    string dateStr = row.Cell(1).GetValue<string>().Trim();
                    string fn = row.Cell(2).GetValue<string>().Trim().ToUpper();
                    string dep = row.Cell(3).GetValue<string>().Trim().ToUpper();
                    string arr = row.Cell(4).GetValue<string>().Trim().ToUpper();
                    string tlc = row.Cell(5).GetValue<string>().Trim();
                    string nom = row.Cell(6).GetValue<string>().Trim();
                    string prenom = row.Cell(7).GetValue<string>().Trim();
                    string rank = row.Cell(8).GetValue<string>().Trim();

                    if (!string.IsNullOrEmpty(tlc) && !string.IsNullOrEmpty(fn))
                    {
                        DateTime dateOrigin = DateTime.TryParse(dateStr, out DateTime d) ? d : DateTime.Today;

                        _db.PNCs.Add(new PNC
                        {
                            Day_of_origin = dateOrigin,
                            FlightNumber = fn,
                            departure = dep,
                            destination = arr,
                            TLC = tlc,
                            name = nom,
                            First_name = prenom,
                            Rank = rank
                        });
                        ajoutes++;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Importation terminée : {ajoutes} Crews ajoutés.";
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur lors de l'importation. Vérifiez que le fichier correspond au modèle. Détail: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
