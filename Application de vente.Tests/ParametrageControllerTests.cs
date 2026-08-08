using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Controllers;
using ApplicationDeVente.Data;
using ApplicationDeVente.Models;
using Moq;
using Xunit;

namespace ApplicationDeVente.Tests
{
    public class ParametrageControllerTests
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

        private static ParametrageController CreateController(ApplicationDbContext db)
        {
            var controller = new ParametrageController(db);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            return controller;
        }

        private static Article CreateArticle(int id = 1, string code = "ART01") => new Article
        {
            Id = id,
            CodeArticle = code,
            NomArticle = "Parfum Test",
            PrixUnitaire = 50m,
            DateDebut = DateTime.Today.AddDays(-10),
            DateFin = DateTime.Today.AddDays(30)
        };

        private static TauxChange CreateTaux(int id = 1) => new TauxChange
        {
            Id = id,
            DeviseSource = "EUR",
            DeviseCible = "TND",
            Taux = 3.4m,
            DateDebut = DateTime.Today.AddDays(-5),
            DateFin = DateTime.Today.AddDays(25)
        };

        // ─── Articles GET ─────────────────────────────────────────────

        [Fact]
        public async Task Articles_ReturnsViewWithListOfArticles()
        {
            using var db = GetInMemoryDbContext();
            db.Articles.Add(CreateArticle(1, "ART01"));
            db.Articles.Add(CreateArticle(2, "ART02"));
            await db.SaveChangesAsync();

            var result = await CreateController(db).Articles();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Article>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Articles_ReturnsEmptyView_WhenNoArticles()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).Articles();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Article>>(viewResult.Model);
            Assert.Empty(model);
        }

        // ─── CreerArticle GET ─────────────────────────────────────────

        [Fact]
        public void CreerArticle_GET_ReturnsViewWithDefaultArticle()
        {
            using var db = GetInMemoryDbContext();
            var result = CreateController(db).CreerArticle();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Article>(viewResult.Model);
            Assert.Equal(DateTime.Today, model.DateDebut);
        }

        // ─── CreerArticle POST ────────────────────────────────────────

        [Fact]
        public async Task CreerArticle_POST_ValidModel_SavesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            var article = new Article
            {
                CodeArticle = "ART01",
                NomArticle = "Chocolat",
                PrixUnitaire = 10m,
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(6)
            };

            var result = await CreateController(db).CreerArticle(article);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
            Assert.Equal(1, await db.Articles.CountAsync());
        }

        [Fact]
        public async Task CreerArticle_POST_DateFinBeforeDateDebut_ReturnsViewWithError()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            var article = new Article
            {
                CodeArticle = "ART02",
                NomArticle = "Test",
                PrixUnitaire = 5m,
                DateDebut = DateTime.Today.AddDays(10),
                DateFin = DateTime.Today // DateFin < DateDebut
            };

            var result = await controller.CreerArticle(article);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task CreerArticle_POST_InvalidModel_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            controller.ModelState.AddModelError("CodeArticle", "Required");

            var result = await controller.CreerArticle(new Article());

            Assert.IsType<ViewResult>(result);
        }

        // ─── ModifierArticle GET ──────────────────────────────────────

        [Fact]
        public async Task ModifierArticle_GET_ReturnsViewWithArticle()
        {
            using var db = GetInMemoryDbContext();
            db.Articles.Add(CreateArticle(1, "ART01"));
            await db.SaveChangesAsync();

            var result = await CreateController(db).ModifierArticle(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Article>(viewResult.Model);
            Assert.Equal("ART01", model.CodeArticle);
        }

        [Fact]
        public async Task ModifierArticle_GET_ReturnsNotFound_WhenArticleDoesNotExist()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ModifierArticle(999);
            Assert.IsType<NotFoundResult>(result);
        }

        // ─── ModifierArticle POST ─────────────────────────────────────

        [Fact]
        public async Task ModifierArticle_POST_ValidModel_UpdatesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            db.Articles.Add(CreateArticle(1, "ART01"));
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var updated = new Article
            {
                Id = 1,
                CodeArticle = "ART01-MOD",
                NomArticle = "Parfum Modifié",
                PrixUnitaire = 75m,
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(6)
            };

            var result = await CreateController(db).ModifierArticle(1, updated);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
        }

        [Fact]
        public async Task ModifierArticle_POST_IdMismatch_ReturnsNotFound()
        {
            using var db = GetInMemoryDbContext();
            var article = new Article { Id = 99, CodeArticle = "X" };
            var result = await CreateController(db).ModifierArticle(1, article);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ModifierArticle_POST_InvalidModel_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            controller.ModelState.AddModelError("Error", "Invalid");

            var article = new Article { Id = 1, CodeArticle = "ART01" };
            var result = await controller.ModifierArticle(1, article);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ModifierArticle_POST_DateFinBeforeDateDebut_ReturnsViewWithError()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            var article = new Article
            {
                Id = 1,
                CodeArticle = "ART01",
                NomArticle = "Test",
                PrixUnitaire = 5m,
                DateDebut = DateTime.Today.AddDays(10),
                DateFin = DateTime.Today
            };

            var result = await controller.ModifierArticle(1, article);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // ─── SupprimerArticle ─────────────────────────────────────────

        [Fact]
        public async Task SupprimerArticle_ExistingArticle_DeletesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            db.Articles.Add(CreateArticle(1, "ART01"));
            await db.SaveChangesAsync();

            var result = await CreateController(db).SupprimerArticle(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
            Assert.Equal(0, await db.Articles.CountAsync());
        }

        [Fact]
        public async Task SupprimerArticle_NonExistingArticle_StillRedirects()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).SupprimerArticle(999);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
        }

        // ─── ImporterArticles ─────────────────────────────────────────

        [Fact]
        public async Task ImporterArticles_NullFile_RedirectsWithError()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).ImporterArticles(null!);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
        }

        [Fact]
        public async Task ImporterArticles_InvalidExtension_RedirectsWithError()
        {
            using var db = GetInMemoryDbContext();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("fichier.txt");
            mockFile.Setup(f => f.Length).Returns(100);

            var controller = CreateController(db);
            var result = await controller.ImporterArticles(mockFile.Object);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
        }

        [Fact]
        public async Task ImporterArticles_InvalidExcelContent_RedirectsWithError()
        {
            using var db = GetInMemoryDbContext();
            var content = new byte[] { 1, 2, 3 }; // Contenu non valide
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("fichier.xlsx");
            mockFile.Setup(f => f.Length).Returns(content.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Callback<Stream, System.Threading.CancellationToken>((s, _) => s.Write(content));

            var controller = CreateController(db);
            var result = await controller.ImporterArticles(mockFile.Object);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Articles", redirect.ActionName);
        }

        // ─── TauxChange GET ───────────────────────────────────────────

        [Fact]
        public async Task TauxChange_ReturnsViewWithListOfTaux()
        {
            using var db = GetInMemoryDbContext();
            db.TauxChanges.Add(CreateTaux(1));
            db.TauxChanges.Add(CreateTaux(2));
            await db.SaveChangesAsync();

            var result = await CreateController(db).TauxChange();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TauxChange>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        // ─── CreerTaux GET ────────────────────────────────────────────

        [Fact]
        public void CreerTaux_GET_ReturnsViewWithDefaultModel()
        {
            using var db = GetInMemoryDbContext();
            var result = CreateController(db).CreerTaux();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TauxChange>(viewResult.Model);
            Assert.Equal("EUR", model.DeviseSource);
            Assert.Equal("TND", model.DeviseCible);
        }

        // ─── CreerTaux POST ───────────────────────────────────────────

        [Fact]
        public async Task CreerTaux_POST_ValidModel_SavesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            var taux = new TauxChange
            {
                DeviseSource = "EUR",
                DeviseCible = "TND",
                Taux = 3.5m,
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(1)
            };

            var result = await CreateController(db).CreerTaux(taux);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("TauxChange", redirect.ActionName);
            Assert.Equal(1, await db.TauxChanges.CountAsync());
        }

        [Fact]
        public async Task CreerTaux_POST_DateFinBeforeDateDebut_ReturnsViewWithError()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            var taux = new TauxChange
            {
                DeviseSource = "EUR",
                DeviseCible = "TND",
                Taux = 3.5m,
                DateDebut = DateTime.Today.AddDays(10),
                DateFin = DateTime.Today
            };

            var result = await controller.CreerTaux(taux);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task CreerTaux_POST_InvalidModel_ReturnsView()
        {
            using var db = GetInMemoryDbContext();
            var controller = CreateController(db);
            controller.ModelState.AddModelError("Taux", "Required");

            var result = await controller.CreerTaux(new TauxChange());

            Assert.IsType<ViewResult>(result);
        }

        // ─── SupprimerTaux ────────────────────────────────────────────

        [Fact]
        public async Task SupprimerTaux_ExistingTaux_DeletesAndRedirects()
        {
            using var db = GetInMemoryDbContext();
            db.TauxChanges.Add(CreateTaux(1));
            await db.SaveChangesAsync();

            var result = await CreateController(db).SupprimerTaux(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("TauxChange", redirect.ActionName);
            Assert.Equal(0, await db.TauxChanges.CountAsync());
        }

        [Fact]
        public async Task SupprimerTaux_NonExistingTaux_StillRedirects()
        {
            using var db = GetInMemoryDbContext();
            var result = await CreateController(db).SupprimerTaux(999);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("TauxChange", redirect.ActionName);
        }
    }
}
