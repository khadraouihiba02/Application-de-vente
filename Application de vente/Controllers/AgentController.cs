using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Agent";
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
    }
}
