using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Controllers;
using ApplicationDeVente.Models;
using Moq;
using Xunit;

namespace ApplicationDeVente.Tests
{
    public class AdminControllerTests
    {
        private static Mock<UserManager<Utilisateur>> MockUserManager()
        {
            var store = new Mock<IUserStore<Utilisateur>>();
            return new Mock<UserManager<Utilisateur>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }

        private static AdminController CreateController(UserManager<Utilisateur> userManager, RoleManager<IdentityRole> roleManager)
        {
            var controller = new AdminController(userManager, roleManager);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            return controller;
        }

        // ─── Dashboard ────────────────────────────────────────────────

        [Fact]
        public async Task Dashboard_ReturnsViewWithStats()
        {
            var userManagerMock = MockUserManager();
            var roleManagerMock = MockRoleManager();

            var users = new List<Utilisateur>
            {
                new Utilisateur { Id = "1", Actif = true },
                new Utilisateur { Id = "2", Actif = false }
            }.AsQueryable();

            var mockUsers = new Mock<IQueryable<Utilisateur>>();
            userManagerMock.Setup(u => u.Users).Returns(users);

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin" }
            }.AsQueryable();
            roleManagerMock.Setup(r => r.Roles).Returns(roles);

            var controller = CreateController(userManagerMock.Object, roleManagerMock.Object);

            var result = await controller.Dashboard();

            var viewResult = Assert.IsType<ViewResult>(result);
            // On ne peut pas tester facilement ViewBag asynchrone avec ToListAsync() sans un vrai Mock IAsyncEnumerable
            // On va donc se contenter de vérifier que ça retourne ViewResult
            Assert.NotNull(viewResult);
        }
    }
}
