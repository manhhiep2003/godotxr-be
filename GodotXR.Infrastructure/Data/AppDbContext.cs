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

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GodotXR.Domain.Entities.Program> Programs { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<ExerciseType> ExerciseTypes { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            FluentApiConfiguration.Configure(modelBuilder);
            DbSeeder.Seed(modelBuilder);
        }
    }
}