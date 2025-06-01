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
            base.OnModelCreating(builder);
        }
    }
}
