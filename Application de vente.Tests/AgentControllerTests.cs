using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Controllers;
using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using ApplicationDeVente.Models.ViewModels;
using Moq;
using Xunit;

namespace ApplicationDeVente.Tests
{
    public class AgentControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var db = new ApplicationDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        private static AgentController CreateController(ApplicationDbContext db)
        {
            var controller = new AgentController(db);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            return controller;
        }

        private static PNC CreatePnc(int id = 1) => new PNC
        {
            Id = id, TLC = "BA1", Rank = "PNC",
            name = "Ben Ali", First_name = "Ahmed"
        };

        private static Article CreateArticle(int id = 1) => new Article
        {
            Id = id, CodeArticle = $"ART0{id}",
            NomArticle = "Parfum Test",
            PrixUnitaire = 50m,
            DateDebut = DateTime.Today.AddDays(-10),
            DateFin = DateTime.Today.AddDays(30)
        };

        private static EtatDesVentes CreateEtatVentes(int id, PNC pnc) => new EtatDesVentes
        {
            Id = id,
            NumeroFeuilleLigne = $"FL{id}00",
            DateVol = DateTime.Today,
            PNCVendeurId = pnc.Id,
            PNCVendeur = pnc,
            Statut = "Saisi",
            MontantEncaisseTND = 200m
        };

        // ─── Dashboard ───────────────────────────────────────────────────

        [Fact]
        public async Task Dashboard_ReturnsViewResult_WithViewModel()
        {
            using var db = GetInMemoryDbContext();
            var pnc = CreatePnc();
            db.PNCs.Add(pnc);
            db.EtatsDesVentes.Add(CreateEtatVentes(1, pnc));
            await db.SaveChangesAsync();

            var result = await CreateController(db).Dashboard();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AgentDashboardViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(1, model.EtatsEnAttente);
        }

        [Fact]
        public async Task Dashboard_WithNoData_ReturnsEmptyViewModel()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).Dashboard();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AgentDashboardViewModel>(viewResult.Model);
            Assert.Equal(0, model.EtatsEnAttente);
            Assert.Equal(0, model.EtatsValides);
            Assert.Empty(model.DerniersEtats);
        }

        [Fact]
        public void Index_RedirectsToDashboard()
        {
            using var db = GetInMemoryDbContext();
            var result = CreateController(db).Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
        }

        // ─── SaisirVentes GET ──────────────────────────────────────────

        [Fact]
        public async Task SaisirVentes_GET_ReturnsViewWithViewModel()
        {
            using var db = GetInMemoryDbContext();
            db.Vols.Add(new Vol
            {
                Id = 1, FN_NUMBER = "TU101",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).SaisirVentes();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SaisieVentesViewModel>(viewResult.Model);
            Assert.Single(model.VolsDisponibles);
            Assert.NotNull(model.LignesArticles);
        }

        // ─── SaisirVentes POST ─────────────────────────────────────────

        [Fact]
        public async Task SaisirVentes_POST_InvalidModel_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            controller.ModelState.AddModelError("Error", "Invalid");

            var vm = new SaisieVentesViewModel { VolId = 0 };
            var result = await controller.SaisirVentes(vm);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SaisirVentes_POST_ValidModel_SavesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle(1);
            var pnc = CreatePnc(1);
            db.Articles.Add(article);
            db.PNCs.Add(pnc);
            var vol = new Vol
            {
                Id = 1, FN_NUMBER = "TU102",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            };
            db.Vols.Add(vol);
            await db.SaveChangesAsync();

            var vm = new SaisieVentesViewModel
            {
                VolId = 1,
                NumeroFeuilleLigne = "FL999",
                DateVol = DateTime.Today,
                PNCVendeurId = 1,
                TauxChangeApplique = 3.4m,
                MontantEncaisseReel = 100m,
                LignesArticles = new List<LigneSaisieArticle>
                {
                    new LigneSaisieArticle
                    {
                        ArticleId = 1, QuantiteVendue = 2,
                        PrixUnitaireEUR = 50m, QuantiteDotation = 5, QuantiteCompl = 0
                    }
                }
            };

            var result = await CreateController(db).SaisirVentes(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Equal(1, await db.EtatsDesVentes.CountAsync());
        }

        [Fact]
        public async Task SaisirVentes_POST_VolIdZero_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            var vm = new SaisieVentesViewModel { VolId = 0, LignesArticles = new List<LigneSaisieArticle>() };

            var result = await controller.SaisirVentes(vm);

            Assert.IsType<ViewResult>(result);
        }

        // ─── SaisirVentesFRS GET ───────────────────────────────────────

        [Fact]
        public async Task SaisirVentesFRS_GET_ReturnsViewWithViewModel()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).SaisirVentesFRS();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SaisieVentesFRSViewModel>(viewResult.Model);
            Assert.NotNull(model.LignesArticles);
            Assert.Empty(model.EtatsPNCDisponibles);
        }

        // ─── SaisirVentesFRS POST ──────────────────────────────────────

        [Fact]
        public async Task SaisirVentesFRS_POST_InvalidModel_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            controller.ModelState.AddModelError("Error", "Invalid");

            var vm = new SaisieVentesFRSViewModel { EtatDesVentesId = 0 };
            var result = await controller.SaisirVentesFRS(vm);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SaisirVentesFRS_POST_ValidModel_SavesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            var pnc = CreatePnc();
            db.PNCs.Add(pnc);
            db.EtatsDesVentes.Add(CreateEtatVentes(1, pnc));
            await db.SaveChangesAsync();

            var vm = new SaisieVentesFRSViewModel
            {
                EtatDesVentesId = 1,
                NumeroEtat = "FRS001",
                DateReception = DateTime.Today,
                TauxChangeApplique = 3.4m,
                MontantDeclareReelTND = 100m,
                LignesArticles = new List<LigneSaisieVenteFRS>
                {
                    new LigneSaisieVenteFRS
                    {
                        CodeArticle = "ART01", Designation = "Parfum Test",
                        QuantiteVendueFRS = 3, PrixUnitaireFRS = 20m
                    }
                }
            };

            var result = await CreateController(db).SaisirVentesFRS(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Equal(1, await db.EtatsDesVentesFRS.CountAsync());
        }

        // ─── GetDetailsVente ───────────────────────────────────────────

        [Fact]
        public async Task GetDetailsVente_ReturnsJsonResult_WhenVenteExists()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle();
            var pnc = CreatePnc();
            db.Articles.Add(article);
            db.PNCs.Add(pnc);
            db.EtatsDesVentes.Add(new EtatDesVentes
            {
                Id = 10, NumeroFeuilleLigne = "FL200", DateVol = DateTime.Today,
                PNCVendeurId = 1, PNCVendeur = pnc, Statut = "Saisi",
                MontantEncaisseTND = 150m,
                Lignes = new List<LigneVente>
                {
                    new LigneVente { Id = 1, ArticleId = 1, Article = article, QuantiteVendue = 2, PrixUnitaireEUR = 50m }
                }
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsVente(10);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetDetailsVente_ReturnsNotFound_WhenVenteDoesNotExist()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetDetailsVente(999);
            Assert.IsType<NotFoundResult>(result);
        }

        // ─── GetDetailsOffre ───────────────────────────────────────────

        [Fact]
        public async Task GetDetailsOffre_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetDetailsOffre(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDetailsOffre_ReturnsJson_WhenExists()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle();
            db.Articles.Add(article);
            db.EtatsDesOffres.Add(new EtatDesOffres
            {
                Id = 5, NumeroFeuilleLigne = "FL300", DateVol = DateTime.Today,
                Statut = "Saisi", ChiffreAffairesEUR = 100m,
                Lignes = new List<LigneOffre>
                {
                    new LigneOffre { Id = 1, ArticleId = 1, Article = article, QuantiteOfferte = 2, PrixUnitairePromoEUR = 30m }
                }
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsOffre(5);
            Assert.IsType<JsonResult>(result);
        }

        // ─── GetDetailsVenteFRS ────────────────────────────────────────
        
        [Fact]
        public async Task GetDetailsVenteFRS_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetDetailsVenteFRS(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDetailsVenteFRS_ReturnsJson_WhenExists()
        {
            using var db = GetInMemoryDbContext();
            var etatVente = new EtatDesVentes { NumeroFeuilleLigne = "FL123", Statut = "Saisi" };
            db.EtatsDesVentes.Add(etatVente);
            await db.SaveChangesAsync();

            var etatFRS = new EtatDesVentesFRS
            {
                EtatDesVentesId = etatVente.Id,
                NumeroEtat = "FLFRS1", DateReception = DateTime.Today,
                StatutControle = "Saisi", ChiffreAffairesEUR = 50m,
                Lignes = new List<LigneVenteFRS>
                {
                    new LigneVenteFRS { CodeArticle = "ART1", NomArticle = "Test", QuantiteVendueFRS = 1, PrixUnitaireFRS = 50m }
                }
            };
            db.EtatsDesVentesFRS.Add(etatFRS);
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsVenteFRS(etatFRS.Id);
            Assert.IsType<JsonResult>(result);
        }

        // ─── GetDetailsOffreFRS ────────────────────────────────────────

        [Fact]
        public async Task GetDetailsOffreFRS_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetDetailsOffreFRS(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDetailsOffreFRS_ReturnsJson_WhenExists()
        {
            using var db = GetInMemoryDbContext();
            var etatOffre = new EtatDesOffres { NumeroFeuilleLigne = "FL123", Statut = "Saisi" };
            db.EtatsDesOffres.Add(etatOffre);
            await db.SaveChangesAsync();

            var etatFRS = new EtatDesOffresFRS
            {
                EtatDesOffresId = etatOffre.Id,
                NumeroEtat = "FLFRS2", DateReception = DateTime.Today,
                StatutControle = "Saisi",
                Lignes = new List<LigneOffreFRS>
                {
                    new LigneOffreFRS { CodeArticle = "ART1", NomArticle = "Test", DotationInitialeFRS = 1 }
                }
            };
            db.EtatsDesOffresFRS.Add(etatFRS);
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsOffreFRS(etatFRS.Id);
            Assert.IsType<JsonResult>(result);
        }

        // ─── GetCrewsByVol ─────────────────────────────────────────────

        [Fact]
        public async Task GetCrewsByVol_ReturnsEmptyJson_WhenVolIdIsZero()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetCrewsByVol(0);
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        [Fact]
        public async Task GetCrewsByVol_ReturnsEmptyJson_WhenVolNotFound()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetCrewsByVol(999);
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        [Fact]
        public async Task GetCrewsByVol_ReturnsCrews_WhenVolExists()
        {
            using var db = GetInMemoryDbContext();
            var vol = new Vol
            {
                Id = 1, FN_NUMBER = "TU200",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            };
            db.Vols.Add(vol);
            db.PNCs.Add(new PNC
            {
                Id = 1, TLC = "BA1", Rank = "PNC",
                name = "Test", First_name = "User",
                FlightNumber = "TU200",
                Day_of_origin = DateTime.Today
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetCrewsByVol(1);
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        // ─── GetVolsByDate ─────────────────────────────────────────────

        [Fact]
        public async Task GetVolsByDate_ReturnsEmptyJson_WhenInvalidDate()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).GetVolsByDate("not-a-date");
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        [Fact]
        public async Task GetVolsByDate_ReturnsVols_WhenValidDate()
        {
            using var db = GetInMemoryDbContext();
            db.Vols.Add(new Vol
            {
                Id = 1, FN_NUMBER = "TU300",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetVolsByDate(DateTime.Today.ToString("yyyy-MM-dd"));
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        // ─── RechercherArticle ─────────────────────────────────────────

        [Fact]
        public async Task RechercherArticle_ReturnsEmpty_WhenQueryIsNullOrEmpty()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).RechercherArticle("");
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        [Fact]
        public async Task RechercherArticle_ReturnsArticles_WhenQueryMatches()
        {
            using var db = GetInMemoryDbContext();
            db.Articles.Add(CreateArticle(1));
            await db.SaveChangesAsync();

            var result = await CreateController(db).RechercherArticle("parfum");
            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

        // ─── ExporterVenteCSV ──────────────────────────────────────────

        [Fact]
        public async Task ExporterVenteCSV_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ExporterVenteCSV(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExporterVenteCSV_ReturnsFileResult_WhenVenteExists()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle(2);
            var pnc = CreatePnc(1);
            db.Articles.Add(article);
            db.PNCs.Add(pnc);
            db.EtatsDesVentes.Add(new EtatDesVentes
            {
                Id = 5, NumeroFeuilleLigne = "FL500", DateVol = DateTime.Today,
                PNCVendeurId = 1, PNCVendeur = pnc, Statut = "Clôturé",
                MontantEncaisseTND = 300m,
                Lignes = new List<LigneVente>
                {
                    new LigneVente { Id = 1, ArticleId = 2, Article = article, QuantiteVendue = 5, PrixUnitaireEUR = 10m }
                }
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).ExporterVenteCSV(5);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Contains("Ventes_FL_", fileResult.FileDownloadName);
        }

        // ─── ExporterOffreCSV ──────────────────────────────────────────

        [Fact]
        public async Task ExporterOffreCSV_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ExporterOffreCSV(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExporterOffreCSV_ReturnsFile_WhenOffreExists()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle(3);
            db.Articles.Add(article);
            db.EtatsDesOffres.Add(new EtatDesOffres
            {
                Id = 6, NumeroFeuilleLigne = "FL600", DateVol = DateTime.Today,
                Statut = "Saisi", ChiffreAffairesEUR = 80m,
                Lignes = new List<LigneOffre>
                {
                    new LigneOffre { ArticleId = 3, Article = article, QuantiteOfferte = 2, PrixUnitairePromoEUR = 20m }
                }
            });
            await db.SaveChangesAsync();

            var result = await CreateController(db).ExporterOffreCSV(6);
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
        }

        // ─── ExporterVenteFRSCSSV ──────────────────────────────────────
        
        [Fact]
        public async Task ExporterVenteFRSCSSV_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ExporterVenteFRSCSSV(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExporterVenteFRSCSSV_ReturnsFile_WhenExists()
        {
            using var db = GetInMemoryDbContext();
            var etatVente = new EtatDesVentes { NumeroFeuilleLigne = "FL123", Statut = "Saisi" };
            db.EtatsDesVentes.Add(etatVente);
            await db.SaveChangesAsync();

            var etatFRS = new EtatDesVentesFRS
            {
                EtatDesVentesId = etatVente.Id,
                NumeroEtat = "FLFRS1", DateReception = DateTime.Today,
                StatutControle = "Saisi", ChiffreAffairesEUR = 50m,
                Lignes = new List<LigneVenteFRS>
                {
                    new LigneVenteFRS { CodeArticle = "ART1", NomArticle = "Test", QuantiteVendueFRS = 1, PrixUnitaireFRS = 50m }
                }
            };
            db.EtatsDesVentesFRS.Add(etatFRS);
            await db.SaveChangesAsync();

            var result = await CreateController(db).ExporterVenteFRSCSSV(etatFRS.Id);
            
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Contains("FLFRS1", fileResult.FileDownloadName);
        }

        // ─── ExporterOffreFRSCSSV ──────────────────────────────────────

        [Fact]
        public async Task ExporterOffreFRSCSSV_ReturnsNotFound_WhenNotExists()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ExporterOffreFRSCSSV(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExporterOffreFRSCSSV_ReturnsFile_WhenExists()
        {
            using var db = GetInMemoryDbContext();
            var etatOffre = new EtatDesOffres { NumeroFeuilleLigne = "FL123", Statut = "Saisi" };
            db.EtatsDesOffres.Add(etatOffre);
            await db.SaveChangesAsync();

            var etatFRS = new EtatDesOffresFRS
            {
                EtatDesOffresId = etatOffre.Id,
                NumeroEtat = "FLFRS2", DateReception = DateTime.Today,
                StatutControle = "Saisi",
                Lignes = new List<LigneOffreFRS>
                {
                    new LigneOffreFRS { CodeArticle = "ART1", NomArticle = "Test", DotationInitialeFRS = 1 }
                }
            };
            db.EtatsDesOffresFRS.Add(etatFRS);
            await db.SaveChangesAsync();

            var result = await CreateController(db).ExporterOffreFRSCSSV(etatFRS.Id);
            
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Contains("FLFRS2", fileResult.FileDownloadName);
        }

        // ─── SaisirVentes POST avec offres (AjouterEtatDesOffres) ─────

        [Fact]
        public async Task SaisirVentes_POST_WithOffres_CreatesEtatOffres()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle(1);
            var pnc = CreatePnc(1);
            db.Articles.Add(article);
            db.PNCs.Add(pnc);
            db.Vols.Add(new Vol
            {
                Id = 1, FN_NUMBER = "TU103",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            });
            await db.SaveChangesAsync();

            var vm = new SaisieVentesViewModel
            {
                VolId = 1,
                NumeroFeuilleLigne = "FL888",
                DateVol = DateTime.Today,
                PNCVendeurId = 1,
                TauxChangeApplique = 3.4m,
                MontantEncaisseReel = 100m,
                LignesArticles = new List<LigneSaisieArticle>
                {
                    new LigneSaisieArticle
                    {
                        ArticleId = 1, QuantiteVendue = 2,
                        PrixUnitaireEUR = 50m, QuantiteDotation = 5, QuantiteCompl = 0
                    }
                },
                LignesOffres = new List<LigneSaisieOffre>
                {
                    new LigneSaisieOffre
                    {
                        ArticleId = 1, QuantiteOfferte = 3, QuantiteDotation = 5, QuantiteCompl = 0,
                        PrixUnitairePromoEUR = 10m
                    }
                }
            };

            var result = await CreateController(db).SaisirVentes(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            // EtatDesOffres doit avoir été créé en plus de EtatDesVentes
            Assert.Equal(1, await db.EtatsDesVentes.CountAsync());
            Assert.Equal(1, await db.EtatsDesOffres.CountAsync());
        }

        // ─── SaisirVentesFRS POST avec offres FRS ─────────────────────

        [Fact]
        public async Task SaisirVentesFRS_POST_WithOffresFRS_CreatesEtatOffresFRS()
        {
            using var db = GetInMemoryDbContext();
            var pnc = CreatePnc();
            db.PNCs.Add(pnc);
            var etatVente = CreateEtatVentes(1, pnc);
            db.EtatsDesVentes.Add(etatVente);
            // Créer un EtatDesOffres associé (même FL et même date)
            db.EtatsDesOffres.Add(new EtatDesOffres
            {
                NumeroFeuilleLigne = etatVente.NumeroFeuilleLigne,
                DateVol = etatVente.DateVol,
                Statut = "Saisi"
            });
            await db.SaveChangesAsync();

            var vm = new SaisieVentesFRSViewModel
            {
                EtatDesVentesId = 1,
                NumeroEtat = "FRS002",
                DateReception = DateTime.Today,
                TauxChangeApplique = 3.4m,
                MontantDeclareReelTND = 50m,
                LignesArticles = new List<LigneSaisieVenteFRS>
                {
                    new LigneSaisieVenteFRS
                    {
                        CodeArticle = "ART01", Designation = "Test",
                        QuantiteVendueFRS = 2, PrixUnitaireFRS = 25m
                    }
                },
                LignesOffres = new List<LigneSaisieOffreFRS>
                {
                    new LigneSaisieOffreFRS
                    {
                        CodeArticle = "ART01", Designation = "Test",
                        DotationInitialeFRS = 5, QuantiteRestanteFRS = 3
                    }
                }
            };

            var result = await CreateController(db).SaisirVentesFRS(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Equal(1, await db.EtatsDesVentesFRS.CountAsync());
            Assert.Equal(1, await db.EtatsDesOffresFRS.CountAsync());
        }

        // ─── GetDetailsVente avec VolsList ─────────────────────────────

        [Fact]
        public async Task GetDetailsVente_WithVolsListAndLignes_ReturnsFullJson()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle();
            var pnc = CreatePnc();
            var vol = new Vol
            {
                Id = 1, FN_NUMBER = "TU777",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            };
            db.Articles.Add(article);
            db.PNCs.Add(pnc);
            db.Vols.Add(vol);
            await db.SaveChangesAsync();

            var etat = new EtatDesVentes
            {
                NumeroFeuilleLigne = "FL777", DateVol = DateTime.Today,
                PNCVendeurId = pnc.Id, Statut = "Saisi",
                MontantEncaisseTND = 300m,
                Lignes = new List<LigneVente>
                {
                    new LigneVente { ArticleId = article.Id, QuantiteVendue = 3, PrixUnitaireEUR = 50m }
                }
            };
            db.EtatsDesVentes.Add(etat);
            await db.SaveChangesAsync();
            db.EtatDesVentesVols.Add(new EtatDesVentesVol { EtatDesVentesId = etat.Id, VolId = vol.Id });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsVente(etat.Id);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        // ─── GetDetailsOffre avec VolsList ─────────────────────────────

        [Fact]
        public async Task GetDetailsOffre_WithVolsListAndLignes_ReturnsFullJson()
        {
            using var db = GetInMemoryDbContext();
            var article = CreateArticle();
            var vol = new Vol
            {
                Id = 1, FN_NUMBER = "TU888",
                DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG",
                DAY_OF_ORIGIN = DateTime.Today, Actif = true
            };
            db.Articles.Add(article);
            db.Vols.Add(vol);
            await db.SaveChangesAsync();

            var etat = new EtatDesOffres
            {
                NumeroFeuilleLigne = "FL888", DateVol = DateTime.Today,
                Statut = "Saisi", ChiffreAffairesEUR = 200m,
                Lignes = new List<LigneOffre>
                {
                    new LigneOffre { ArticleId = article.Id, QuantiteOfferte = 2, PrixUnitairePromoEUR = 30m }
                }
            };
            db.EtatsDesOffres.Add(etat);
            await db.SaveChangesAsync();
            db.EtatDesOffresVols.Add(new EtatDesOffresVol { EtatDesOffresId = etat.Id, VolId = vol.Id });
            await db.SaveChangesAsync();

            var result = await CreateController(db).GetDetailsOffre(etat.Id);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }
    }
}
