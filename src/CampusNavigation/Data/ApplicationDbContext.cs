using Microsoft.EntityFrameworkCore;
using System;
using CampusNavigation.Models;

namespace CampusNavigation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserLocation> UserLocations { get; set; } = null!;
        public DbSet<Building> Buildings { get; set; } = null!;
        public DbSet<BuildingConnection> BuildingConnections { get; set; } = null!;
        public DbSet<UserPresence> UserPresences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Building>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<BuildingConnection>(entity =>
            {
                entity.HasOne(d => d.FromBuilding)
                    .WithMany(b => b.OutgoingConnections)
                    .HasForeignKey(d => d.FromBuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToBuilding)
                    .WithMany(b => b.IncomingConnections)
                    .HasForeignKey(d => d.ToBuildingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Building>()
                .HasIndex(b => b.Name);
            
            modelBuilder.Entity<BuildingConnection>()
                .HasIndex(bc => new { bc.FromBuildingId, bc.ToBuildingId });
            
            modelBuilder.Entity<UserLocation>()
                .HasIndex(ul => ul.Timestamp);

            modelBuilder.Entity<UserPresence>(entity =>
            {
                entity.HasIndex(up => up.UserId).IsUnique(); 
                entity.HasIndex(up => up.CurrentBuildingId);

                entity.HasOne(up => up.CurrentBuilding)
                    .WithMany() 
                    .HasForeignKey(up => up.CurrentBuildingId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
}