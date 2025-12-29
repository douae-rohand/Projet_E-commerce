using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Models;

namespace Projet__E_commerce.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets pour chaque entité
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<AdresseLivraison> AdressesLivraison { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Variante> Variantes { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<LigneCommande> LignesCommande { get; set; }
        public DbSet<Livraison> Livraisons { get; set; }
        public DbSet<BordereauLivraison> BordereauxLivraison { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<Avis> Avis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations et contraintes

            // One-to-One relationships
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Utilisateur)
                .WithOne()
                .HasForeignKey<Admin>(a => a.id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasOne(c => c.Utilisateur)
                .WithOne()
                .HasForeignKey<Client>(c => c.id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SuperAdmin>()
                .HasOne(s => s.Utilisateur)
                .WithOne()
                .HasForeignKey<SuperAdmin>(s => s.id)
                .OnDelete(DeleteBehavior.Cascade);

            // Client-AdresseLivraison relationship
            modelBuilder.Entity<AdresseLivraison>()
                .HasOne(al => al.Client)
                .WithMany(c => c.AdressesLivraison)
                .HasForeignKey(al => al.idClient)
                .OnDelete(DeleteBehavior.Cascade);

            // Categorie-Produit relationship
            modelBuilder.Entity<Produit>()
                .HasOne(p => p.Categorie)
                .WithMany(c => c.Produits)
                .HasForeignKey(p => p.idC)
                .OnDelete(DeleteBehavior.Restrict);

            // Admin-Produit relationship
            modelBuilder.Entity<Produit>()
                .HasOne(p => p.Admin)
                .WithMany(a => a.Produits)
                .HasForeignKey(p => p.idAdmin)
                .OnDelete(DeleteBehavior.Restrict);

            // Produit-Variante relationship
            modelBuilder.Entity<Variante>()
                .HasOne(v => v.Produit)
                .WithMany(p => p.Variantes)
                .HasForeignKey(v => v.idP)
                .OnDelete(DeleteBehavior.Cascade);

            // Client-Commande relationship
            modelBuilder.Entity<Commande>()
                .HasOne(co => co.Client)
                .WithMany(c => c.Commandes)
                .HasForeignKey(co => co.idClient)
                .OnDelete(DeleteBehavior.Cascade);

            // Commande-LigneCommande relationship
            modelBuilder.Entity<LigneCommande>()
                .HasOne(lc => lc.Commande)
                .WithMany(co => co.LignesCommande)
                .HasForeignKey(lc => lc.idCommande)
                .OnDelete(DeleteBehavior.Cascade);

            // Variante-LigneCommande relationship
            modelBuilder.Entity<LigneCommande>()
                .HasOne(lc => lc.Variante)
                .WithMany(v => v.LignesCommande)
                .HasForeignKey(lc => lc.idV)
                .OnDelete(DeleteBehavior.Restrict);

            // Commande-Livraison relationship
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.Commande)
                .WithOne(co => co.Livraison)
                .HasForeignKey<Livraison>(l => l.idCommande)
                .OnDelete(DeleteBehavior.Cascade);

            // AdresseLivraison-Livraison relationship
            modelBuilder.Entity<Livraison>()
                .HasOne(l => l.AdresseLivraison)
                .WithMany(al => al.Livraisons)
                .HasForeignKey(l => l.idAdresse)
                .OnDelete(DeleteBehavior.Restrict);

            // Livraison-BordereauLivraison relationship
            modelBuilder.Entity<BordereauLivraison>()
                .HasOne(bl => bl.Livraison)
                .WithOne(l => l.BordereauLivraison)
                .HasForeignKey<BordereauLivraison>(bl => bl.idLivraison)
                .OnDelete(DeleteBehavior.Cascade);

            // Commande-Facture relationship
            modelBuilder.Entity<Facture>()
                .HasOne(f => f.Commande)
                .WithOne(co => co.Facture)
                .HasForeignKey<Facture>(f => f.idCommande)
                .OnDelete(DeleteBehavior.Cascade);

            // Client-Avis relationship
            modelBuilder.Entity<Avis>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Avis)
                .HasForeignKey(a => a.idClient)
                .OnDelete(DeleteBehavior.Cascade);

            // Produit-Avis relationship
            modelBuilder.Entity<Avis>()
                .HasOne(a => a.Produit)
                .WithMany(p => p.Avis)
                .HasForeignKey(a => a.idProduit)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration des propriétés décimales
            modelBuilder.Entity<Variante>()
                .Property(v => v.prix)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Commande>()
                .Property(co => co.prixTotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<LigneCommande>()
                .Property(lc => lc.prix_unitaire)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Livraison>()
                .Property(l => l.frais)
                .HasPrecision(10, 2);

            // Configuration des contraintes CHECK
            modelBuilder.Entity<Categorie>()
                .ToTable(t => t.HasCheckConstraint("CK_Categorie_Nom", "nom IN ('cosmetique', 'nutritive', 'medical', 'artisanat', 'agricole')"));

            modelBuilder.Entity<Produit>()
                .ToTable(t => t.HasCheckConstraint("CK_Produit_Statut", "statut IN ('active', 'inactive')"));

            modelBuilder.Entity<Commande>()
                .ToTable(t => t.HasCheckConstraint("CK_Commande_Statut", "statut IN ('en_attente', 'en_preparation', 'en_livraison', 'livre', 'annule')"));

            modelBuilder.Entity<Livraison>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Livraison_Statut", "statut IN ('en_preparation', 'en_cours', 'livre', 'non_livre')");
                    t.HasCheckConstraint("CK_Livraison_ModeLivraison", "mode_livraison IN ('Standard', 'Express')");
                });

            modelBuilder.Entity<Avis>()
                .ToTable(t => t.HasCheckConstraint("CK_Avis_Note", "note BETWEEN 1 AND 5"));

            // Seed data (données initiales)
            modelBuilder.Entity<Categorie>().HasData(
                new Categorie { idC = 1, nom = "cosmetique", description = "Produits cosmétiques et de beauté" },
                new Categorie { idC = 2, nom = "nutritive", description = "Produits nutritionnels et alimentaires" },
                new Categorie { idC = 3, nom = "medical", description = "Produits médicaux et de santé" },
                new Categorie { idC = 4, nom = "artisanat", description = "Produits artisanaux locaux" },
                new Categorie { idC = 5, nom = "agricole", description = "Produits agricoles et fermiers" }
            );
        }
    }
}
