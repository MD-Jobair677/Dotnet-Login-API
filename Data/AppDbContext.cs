using Microsoft.EntityFrameworkCore;
using LoginSystem.Models;
using Microsoft.EntityFrameworkCore.Internal;


namespace LoginSystem.Data
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



        }
        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentProfile> StudentProfiles{get;set;}

    }
}