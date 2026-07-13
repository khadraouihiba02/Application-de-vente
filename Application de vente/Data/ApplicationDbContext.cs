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

        public DbSet<Article> Articles { get; set; }
        public DbSet<Contrat> Contrats { get; set; }
        public DbSet<PrixArticleContrat> PrixArticleContrats { get; set; }
        public DbSet<TauxChange> TauxChanges { get; set; }
    }
}
