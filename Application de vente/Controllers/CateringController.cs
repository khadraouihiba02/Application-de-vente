using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Catering")]
    public class CateringController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Direction Catering";
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
    }
}
