using Microsoft.EntityFrameworkCore;
using LoginSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Internal;


namespace LoginSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();
            modelBuilder.Entity<Student>()
                        .HasOne(s => s.Profile)
                        .WithOne(p => p.Student)
                        .HasForeignKey<StudentProfile>(p => p.StudentId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserRole>()
.HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<RolePermission>()
                .HasKey(x => new { x.RoleId, x.PermissionId });

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);



        }
        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

    }
}