using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "DCF")]
    public class DCFController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — DCF";
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
    }
}
