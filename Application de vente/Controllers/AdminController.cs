using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Admin";
            return View();
        }

        public IActionResult GestionUsers()
        {
            ViewData["Title"] = "Gestion des Utilisateurs";
            return View();
        }
    }
}
