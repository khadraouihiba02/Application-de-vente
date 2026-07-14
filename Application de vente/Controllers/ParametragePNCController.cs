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
            ViewData["Title"] = "Gestion des PNC";
            var pncs = await _db.PNCs.OrderBy(p => p.Nom).ToListAsync();
            return View(pncs);
        }

        // ── Créer PNC ─────────────────────────────────────────────────
        public IActionResult Creer()
        {
            ViewData["Title"] = "Nouveau PNC";
            return View(new PNC());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(PNC model)
        {
            if (_db.PNCs.Any(p => p.Matricule == model.Matricule))
                ModelState.AddModelError("Matricule", "Ce matricule existe déjà.");

            if (ModelState.IsValid)
            {
                _db.PNCs.Add(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"PNC {model.Prenom} {model.Nom} ({model.Matricule}) ajouté avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Nouveau PNC";
            return View(model);
        }

        // ── Modifier PNC ──────────────────────────────────────────────
        public async Task<IActionResult> Modifier(int id)
        {
            ViewData["Title"] = "Modifier PNC";
            var pnc = await _db.PNCs.FindAsync(id);
            if (pnc == null) return NotFound();
            return View(pnc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(PNC model)
        {
            if (_db.PNCs.Any(p => p.Matricule == model.Matricule && p.Id != model.Id))
                ModelState.AddModelError("Matricule", "Ce matricule est déjà utilisé par un autre PNC.");

            if (ModelState.IsValid)
            {
                _db.PNCs.Update(model);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"PNC {model.Prenom} {model.Nom} modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Modifier PNC";
            return View(model);
        }

        // ── Toggle Actif PNC ──────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActif(int id)
        {
            var pnc = await _db.PNCs.FindAsync(id);
            if (pnc != null)
            {
                pnc.Actif = !pnc.Actif;
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"PNC {pnc.Prenom} {pnc.Nom} {(pnc.Actif ? "activé" : "désactivé")}.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Supprimer(int id)
        {
            var pnc = await _db.PNCs.FindAsync(id);
            if (pnc != null)
            {
                _db.PNCs.Remove(pnc);
                await _db.SaveChangesAsync();
                TempData["Succes"] = $"PNC {pnc.Prenom} {pnc.Nom} ({pnc.Matricule}) a été supprimé.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActionEnMasse(int[] ids, string actionType)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Erreur"] = "Aucun PNC sélectionné.";
                return RedirectToAction(nameof(Index));
            }

            var pncs = await _db.PNCs.Where(p => ids.Contains(p.Id)).ToListAsync();
            
            if (actionType == "Supprimer")
            {
                _db.PNCs.RemoveRange(pncs);
                TempData["Succes"] = $"{pncs.Count} PNC supprimés avec succès.";
            }
            else if (actionType == "Activer")
            {
                pncs.ForEach(p => p.Actif = true);
                TempData["Succes"] = $"{pncs.Count} PNC activés avec succès.";
            }
            else if (actionType == "Desactiver")
            {
                pncs.ForEach(p => p.Actif = false);
                TempData["Succes"] = $"{pncs.Count} PNC désactivés avec succès.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── Import / Export Excel ─────────────────────────────────────
        public IActionResult TelechargerModele()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Modèle PNC");
            
            // En-têtes
            worksheet.Cell(1, 1).Value = "Matricule";
            worksheet.Cell(1, 2).Value = "Nom";
            worksheet.Cell(1, 3).Value = "Prénom";
            
            // Style
            worksheet.Range("A1:C1").Style.Font.Bold = true;
            worksheet.Range("A1:C1").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            worksheet.Columns().AdjustToContents();

            // Exemple
            worksheet.Cell(2, 1).Value = "15070F";
            worksheet.Cell(2, 2).Value = "BEN SALEM";
            worksheet.Cell(2, 3).Value = "KAIS";

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Modele_Import_PNC.xlsx");
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
                int ignores = 0;

                foreach (var row in rows)
                {
                    string matricule = row.Cell(1).GetValue<string>().Trim();
                    string nom = row.Cell(2).GetValue<string>().Trim();
                    string prenom = row.Cell(3).GetValue<string>().Trim();

                    if (!string.IsNullOrEmpty(matricule) && !string.IsNullOrEmpty(nom))
                    {
                        // Vérifier si le matricule existe déjà
                        if (!_db.PNCs.Any(p => p.Matricule == matricule))
                        {
                            _db.PNCs.Add(new PNC
                            {
                                Matricule = matricule,
                                Nom = nom,
                                Prenom = prenom,
                                Actif = true
                            });
                            ajoutes++;
                        }
                        else
                        {
                            ignores++;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Succes"] = $"Importation terminée : {ajoutes} PNC ajoutés, {ignores} ignorés (déjà existants).";
            }
            catch (Exception ex)
            {
                TempData["Erreur"] = "Erreur lors de l'importation. Vérifiez que le fichier correspond au modèle. Détail: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
