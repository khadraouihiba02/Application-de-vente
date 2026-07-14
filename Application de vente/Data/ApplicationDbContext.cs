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
        public DbSet<PNC> PNCs { get; set; }
        public DbSet<Vol> Vols { get; set; }

        // Module Saisie des Ventes
        public DbSet<EtatDesVentes> EtatsDesVentes { get; set; }
        public DbSet<LigneVente> LignesVentes { get; set; }

        // Module Saisie des Offres
        public DbSet<EtatDesOffres> EtatsDesOffres { get; set; }
        public DbSet<LigneOffre> LignesOffres { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration de la précision des décimales
            builder.Entity<TauxChange>()
                .Property(t => t.Taux)
                .HasColumnType("decimal(18,4)");

            builder.Entity<Article>()
                .Property(a => a.PrixUnitaire)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EtatDesVentes>()
                .Property(e => e.ChiffreAffairesEUR)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EtatDesVentes>()
                .Property(e => e.MontantEncaisseTND)
                .HasColumnType("decimal(18,3)"); // Le TND utilise 3 décimales

            builder.Entity<EtatDesVentes>()
                .Property(e => e.TauxChangeApplique)
                .HasColumnType("decimal(18,4)");

            builder.Entity<LigneVente>()
                .Property(l => l.PrixUnitaireEUR)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EtatDesOffres>()
                .Property(e => e.ChiffreAffairesEUR)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EtatDesOffres>()
                .Property(e => e.MontantEncaisseTND)
                .HasColumnType("decimal(18,3)");

            builder.Entity<EtatDesOffres>()
                .Property(e => e.TauxChangeApplique)
                .HasColumnType("decimal(18,4)");

            builder.Entity<LigneOffre>()
                .Property(l => l.PrixUnitairePromoEUR)
                .HasColumnType("decimal(18,2)");
        }
    }
}
