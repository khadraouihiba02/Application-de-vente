using Microsoft.AspNetCore.Identity;
using ApplicationDeVente.Models;

namespace ApplicationDeVente.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilisateur>>();

            // ── Création des 4 rôles du système ──────────────────
            string[] roles = { "Admin", "Agent", "Catering", "DCF" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ── Compte Admin (Responsable IT / Administrateur système) ──
            var defaultAdminEmail = "admin@tunisair.com.tn";
            var defaultAdmin = await userManager.FindByEmailAsync(defaultAdminEmail);

            if (defaultAdmin == null)
            {
                var admin = new Utilisateur
                {
                    UserName = defaultAdminEmail,
                    Email = defaultAdminEmail,
                    Nom = "Administrateur",
                    Prenom = "Système",
                    Actif = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            var defaultAgentEmail = "agent@tunisair.com.tn";
            var defaultAgent = await userManager.FindByEmailAsync(defaultAgentEmail);

            if (defaultAgent == null)
            {
                var agent = new Utilisateur
                {
                    UserName = defaultAgentEmail,
                    Email = defaultAgentEmail,
                    Nom = "Salem",
                    Prenom = "Ben",
                    Actif = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(agent, "Agent@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agent, "Agent");
                }
            }

            var defaultCateringEmail = "catering@tunisair.com.tn";
            var defaultCatering = await userManager.FindByEmailAsync(defaultCateringEmail);

            if (defaultCatering == null)
            {
                var catering = new Utilisateur
                {
                    UserName = defaultCateringEmail,
                    Email = defaultCateringEmail,
                    Nom = "Chahed",
                    Prenom = "Ali",
                    Actif = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(catering, "Catering@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(catering, "Catering");
                }
            }

            var defaultDcfEmail = "dcf@tunisair.com.tn";
            var defaultDcf = await userManager.FindByEmailAsync(defaultDcfEmail);

            if (defaultDcf == null)
            {
                var dcf = new Utilisateur
                {
                    UserName = defaultDcfEmail,
                    Email = defaultDcfEmail,
                    Nom = "Trabelsi",
                    Prenom = "Rania",
                    Actif = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(dcf, "Dcf@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(dcf, "DCF");
                }
            }
        }
    }
}
