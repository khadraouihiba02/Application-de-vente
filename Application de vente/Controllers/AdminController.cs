using ApplicationDeVente.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApplicationDeVente.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Utilisateur> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Utilisateur> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Tableau de bord — Admin";

            var allUsers = await _userManager.Users.ToListAsync();
            ViewBag.TotalUsers   = allUsers.Count;
            ViewBag.ActiveUsers  = allUsers.Count(u => u.Actif);
            ViewBag.InactiveUsers = allUsers.Count(u => !u.Actif);
            ViewBag.TotalRoles   = await _roleManager.Roles.CountAsync();

            return View();
        }

        // GET: /Admin/GestionUsers
        public async Task<IActionResult> GestionUsers()
        {
            ViewData["Title"] = "Gestion des Utilisateurs";
            
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    NomComplet = user.NomComplet,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Aucun",
                    Actif = user.Actif
                });
            }

            return View(userList);
        }

        // GET: /Admin/CreerUser
        public async Task<IActionResult> CreerUser()
        {
            ViewData["Title"] = "Créer un Utilisateur";
            
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles);
            
            return View(new CreerUtilisateurViewModel());
        }

        // POST: /Admin/CreerUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerUser(CreerUtilisateurViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'utilisateur existe déjà
                var existant = await _userManager.FindByEmailAsync(model.Email);
                if (existant != null)
                {
                    ModelState.AddModelError("Email", "Cette adresse email est déjà utilisée.");
                    var rolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    ViewBag.Roles = new SelectList(rolesList);
                    return View(model);
                }

                var user = new Utilisateur
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Actif = true,
                    EmailConfirmed = true // Confirmé par défaut
                };

                var result = await _userManager.CreateAsync(user, model.MotDePasse);
                if (result.Succeeded)
                {
                    // Assigner le rôle
                    if (await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    TempData["Succes"] = $"L'utilisateur '{user.NomComplet}' a été créé avec succès.";
                    return RedirectToAction(nameof(GestionUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles);
            return View(model);
        }

        // GET: /Admin/ModifierUser/5
        public async Task<IActionResult> ModifierUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var rolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(rolesList, roles.FirstOrDefault());

            var model = new ModifierUtilisateurViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = roles.FirstOrDefault() ?? string.Empty,
                Actif = user.Actif
            };

            ViewData["Title"] = "Modifier l'Utilisateur";
            return View(model);
        }

        // POST: /Admin/ModifierUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierUser(string id, ModifierUtilisateurViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.Nom = model.Nom;
                user.Prenom = model.Prenom;
                user.Actif = model.Actif;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Mettre à jour le rôle
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (await _roleManager.RoleExistsAsync(model.Role))
                        {
                            await _userManager.AddToRoleAsync(user, model.Role);
                        }
                    }

                    // Mettre à jour le mot de passe si saisi
                    if (!string.IsNullOrEmpty(model.MotDePasse))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.MotDePasse);
                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            var rolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                            ViewBag.Roles = new SelectList(rolesList, model.Role);
                            return View(model);
                        }
                    }

                    TempData["Succes"] = $"L'utilisateur '{user.NomComplet}' a été modifié avec succès.";
                    return RedirectToAction(nameof(GestionUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = new SelectList(allRoles, model.Role);
            return View(model);
        }

        // POST: /Admin/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Empêcher l'administrateur de se désactiver lui-même
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Erreur"] = "Vous ne pouvez pas désactiver votre propre compte administrateur.";
                return RedirectToAction(nameof(GestionUsers));
            }

            user.Actif = !user.Actif;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                string statut = user.Actif ? "activé" : "désactivé";
                TempData["Succes"] = $"Le compte de '{user.NomComplet}' a été {statut} avec succès.";
            }
            else
            {
                TempData["Erreur"] = "Une erreur est survenue lors de la modification du statut.";
            }

            return RedirectToAction(nameof(GestionUsers));
        }
    }
}
