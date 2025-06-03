using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagementSystem.Web.Entities;

namespace MiniAccountManagementSystem.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Module>(
                entity =>
                {
                    entity.ToTable("Modules");
                    entity.HasKey(m => m.ModuleId);
                    entity.Property(m => m.ModuleName).IsRequired().HasMaxLength(100);
                    entity.HasIndex(m => m.ModuleName).IsUnique();
                });

            builder.Entity<RoleModule>(
                entity =>
                {
                    entity.ToTable("RoleModules");
                });

            builder.Entity<RoleModule>()
            .HasIndex(rm => new { rm.RoleId, rm.ModuleId })
            .IsUnique();

            builder.Entity<RoleModule>()
                .HasOne(rm => rm.Role)
                .WithMany()
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoleModule>()
                .HasOne(rm => rm.Module)
                .WithMany(m => m.RoleModules)
                .HasForeignKey(rm => rm.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ChartOfAccount entity
            builder.Entity<ChartOfAccount>(
                entity =>
                {
                    entity.ToTable("ChartOfAccounts");
                    entity.HasKey(c => c.AccountId);
                    entity.Property(c => c.AccountCode).IsRequired().HasMaxLength(20);
                    entity.Property(c => c.AccountName).IsRequired().HasMaxLength(100);
                    entity.Property(c => c.Description).HasMaxLength(500);
                    entity.HasIndex(c => c.AccountCode).IsUnique();
                    
                    // Self-referencing relationship for parent-child accounts
                    entity.HasOne(c => c.ParentAccount)
                          .WithMany(c => c.ChildAccounts)
                          .HasForeignKey(c => c.ParentAccountId)
                          .IsRequired(false)
                          .OnDelete(DeleteBehavior.Restrict);
                });

            base.OnModelCreating(builder);
        }
    }
}
