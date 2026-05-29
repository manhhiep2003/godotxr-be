using GodotXR.Domain.Entities;
using GodotXR.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GodotXR.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply Fluent API Configurations
            FluentApiConfiguration.Configure(modelBuilder);

            // Seeding Data
            DbSeeder.Seed(modelBuilder);
        }
    }
}