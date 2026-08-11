using System;
using System.Collections.Generic;
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
    public class ParametrageVolControllerTests
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

        private ParametrageVolController CreateControllerWithTempData(ApplicationDbContext db)
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            var controller = new ParametrageVolController(db)
            {
                TempData = tempData
            };
            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfVols()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            db.Vols.Add(new Vol { Id = 1, FN_NUMBER = "TU202", DAY_OF_ORIGIN = DateTime.Today, DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "CDG", Actif = true });
            db.Vols.Add(new Vol { Id = 2, FN_NUMBER = "TU722", DAY_OF_ORIGIN = DateTime.Today, DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "ORY", Actif = true });
            await db.SaveChangesAsync();

            var controller = CreateControllerWithTempData(db);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Vol>>(viewResult.Model);
            Assert.Equal(2, ((List<Vol>)model).Count);
        }

        [Fact]
        public async Task Creer_POST_AddsVol_WhenValid()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = CreateControllerWithTempData(db);
            var newVol = new Vol
            {
                FN_NUMBER = "TU999",
                DAY_OF_ORIGIN = DateTime.Today,
                DEP_AP_ACTUAL = "TUN",
                ARR_AP_ACTUAL = "FRA",
                Actif = true
            };

            // Act
            var result = await controller.Creer(newVol);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, await db.Vols.CountAsync());
        }

        [Fact]
        public async Task Creer_POST_ReturnsView_WhenVolAlreadyExists()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var today = DateTime.Today;
            db.Vols.Add(new Vol { Id = 1, FN_NUMBER = "TU100", DAY_OF_ORIGIN = today, DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "MRS", Actif = true });
            await db.SaveChangesAsync();

            var controller = CreateControllerWithTempData(db);
            var duplicateVol = new Vol { FN_NUMBER = "TU100", DAY_OF_ORIGIN = today, DEP_AP_ACTUAL = "TUN", ARR_AP_ACTUAL = "MRS", Actif = true };

            // Act
            var result = await controller.Creer(duplicateVol);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void TelechargerModele_ReturnsExcelFile()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = CreateControllerWithTempData(db);

            // Act
            var result = controller.TelechargerModele();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("Modele_Import_Vols.xlsx", fileResult.FileDownloadName);
        }
    }
}
