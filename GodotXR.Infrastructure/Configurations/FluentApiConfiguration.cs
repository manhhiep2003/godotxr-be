using GodotXR.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace GodotXR.Infrastructure.Configurations
{
    public static class FluentApiConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            // =========================================================
            // IDENTITY
            // =========================================================
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
               .HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
