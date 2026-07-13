using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApplicationDeVente.Models;

namespace ApplicationDeVente.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilisateur>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Module Paramétrage
        public DbSet<Article> Articles { get; set; }
        public DbSet<TauxChange> TauxChanges { get; set; }
    }
}
