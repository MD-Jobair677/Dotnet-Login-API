using BulkMail.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulkMail.Infrastructure.Persistence.Configurations.User
{
    public class RoleConfiguration : IEntityTypeConfiguration<BulkMail.Domain.User.Entities.Role>
    {
        public void Configure(EntityTypeBuilder<BulkMail.Domain.User.Entities.Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
