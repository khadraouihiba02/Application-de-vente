using ApplicationDeVente.Models;
using Application_de_vente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ApplicationDeVente.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Redirection automatique vers le bon dashboard selon le rôle
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");

            if (User.IsInRole("Agent"))
                return RedirectToAction("Dashboard", "Agent");

            if (User.IsInRole("Catering"))
                return RedirectToAction("Dashboard", "Catering");

            if (User.IsInRole("DCF"))
                return RedirectToAction("Dashboard", "DCF");

            // Si aucun rôle reconnu → retour au login
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
