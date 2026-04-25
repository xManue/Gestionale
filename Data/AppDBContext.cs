using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<WorkType> WorkTypes { get; set; }
        public DbSet<WorkTypeMaterial> WorkTypeMaterials { get; set; }
        public DbSet<WorkTypeTool> WorkTypeTools { get; set; }

        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<InterventionMaterial> InterventionMaterials { get; set; }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<InterventionLog> InterventionLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
             .HasIndex(u => u.Email)
             .IsUnique();

            modelBuilder.Entity<Assignment>()
                .HasIndex(a => new { a.InterventionId, a.UserId })
                .IsUnique();
        }
    }
}
