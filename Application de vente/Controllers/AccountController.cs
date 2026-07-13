using ApplicationDeVente.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDeVente.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Utilisateur> _signInManager;
        private readonly UserManager<Utilisateur> _userManager;

        public AccountController(
            SignInManager<Utilisateur> signInManager,
            UserManager<Utilisateur> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                userName: model.Email,
                password: model.MotDePasse,
                isPersistent: model.SeSouvenir,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null && !user.Actif)
                {
                    await _signInManager.SignOutAsync();
                    TempData["Erreur"] = "Votre compte est désactivé. Contactez l'administrateur.";
                    return View(model);
                }

                var roles = await _userManager.GetRolesAsync(user!);
                var role = roles.FirstOrDefault();

                return role switch
                {
                    "Agent" => RedirectToAction("Index", "Agent"),
                    "Catering" => RedirectToAction("Index", "Catering"),
                    "DCF" => RedirectToAction("Index", "DCF"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            TempData["Erreur"] = "Email ou mot de passe incorrect.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccesRefuse()
        {
            return View();
        }
    }
}
