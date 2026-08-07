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

        [Fact]
        public async Task Dashboard_ReturnsViewResult_WithViewModel()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            
            // Seed test data
            db.EtatsDesVentes.Add(new EtatDesVentes
            {
                Id = 1,
                NumeroFeuilleLigne = "FL101",
                DateVol = DateTime.Today,
                PNCVendeurId = 1,
                Statut = "Saisi",
                MontantEncaisseTND = 200m
            });
            await db.SaveChangesAsync();

            var controller = new AgentController(db);

            // Act
            var result = await controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AgentDashboardViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(1, model.EtatsEnAttente);
        }

        [Fact]
        public async Task GetDetailsVente_ReturnsJsonResult_WhenVenteExists()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var article = new Article { Id = 1, CodeArticle = "ART01", NomArticle = "Parfum Chanel", Description = "Parfum mixte", PrixUnitaire = 50m, DateDebut = DateTime.Today.AddDays(-10), DateFin = DateTime.Today.AddDays(30) };
            db.Articles.Add(article);

            var pnc = new PNC { Id = 1, TLC = "BA1", Rank = "PNC", name = "Ben Ali", First_name = "Ahmed" };
            db.PNCs.Add(pnc);

            var etat = new EtatDesVentes
            {
                Id = 10,
                NumeroFeuilleLigne = "FL200",
                DateVol = DateTime.Today,
                PNCVendeurId = 1,
                PNCVendeur = pnc,
                Statut = "Saisi",
                MontantEncaisseTND = 150m,
                Lignes = new List<LigneVente>
                {
                    new LigneVente { Id = 1, ArticleId = 1, Article = article, QuantiteVendue = 2, PrixUnitaireEUR = 50m }
                }
            };
            db.EtatsDesVentes.Add(etat);
            await db.SaveChangesAsync();

            var controller = new AgentController(db);

            // Act
            var result = await controller.GetDetailsVente(10);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetDetailsVente_ReturnsNotFound_WhenVenteDoesNotExist()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = new AgentController(db);

            // Act
            var result = await controller.GetDetailsVente(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExporterVenteCSV_ReturnsFileResult_WhenVenteExists()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var article = new Article { Id = 2, CodeArticle = "ART02", NomArticle = "Chocolat", Description = "Chocolat noir", PrixUnitaire = 10m, DateDebut = DateTime.Today.AddDays(-10), DateFin = DateTime.Today.AddDays(30) };
            db.Articles.Add(article);

            var pnc = new PNC { Id = 1, TLC = "BA1", Rank = "PNC", name = "Ben Ali", First_name = "Ahmed" };
            db.PNCs.Add(pnc);

            var etat = new EtatDesVentes
            {
                Id = 5,
                NumeroFeuilleLigne = "FL500",
                DateVol = DateTime.Today,
                PNCVendeurId = 1,
                PNCVendeur = pnc,
                Statut = "Clôturé",
                MontantEncaisseTND = 300m,
                Lignes = new List<LigneVente>
                {
                    new LigneVente { Id = 1, ArticleId = 2, Article = article, QuantiteVendue = 5, PrixUnitaireEUR = 10m }
                }
            };
            db.EtatsDesVentes.Add(etat);
            await db.SaveChangesAsync();

            var controller = new AgentController(db);

            // Act
            var result = await controller.ExporterVenteCSV(5);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Contains("Ventes_FL_", fileResult.FileDownloadName);
        }
    }
}
