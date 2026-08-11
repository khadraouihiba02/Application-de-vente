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
    public class ParametragePNCControllerTests
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

        private ParametragePNCController CreateControllerWithTempData(ApplicationDbContext db)
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            return new ParametragePNCController(db)
            {
                TempData = tempData
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfPNC()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            db.PNCs.Add(new PNC { Id = 1, TLC = "TLC01", Rank = "CCP", name = "BEN ALI", First_name = "AHMED", FlightNumber = "TU202", Day_of_origin = DateTime.Today, departure = "TUN", destination = "CDG" });
            db.PNCs.Add(new PNC { Id = 2, TLC = "TLC02", Rank = "PNC", name = "TRABELSI", First_name = "SAMI", FlightNumber = "TU202", Day_of_origin = DateTime.Today, departure = "TUN", destination = "CDG" });
            await db.SaveChangesAsync();

            var controller = CreateControllerWithTempData(db);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<PNC>>(viewResult.Model);
            Assert.Equal(2, ((List<PNC>)model).Count);
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
            Assert.Equal("Modele_Import_CabinCrew.xlsx", fileResult.FileDownloadName);
        }
    }
}
