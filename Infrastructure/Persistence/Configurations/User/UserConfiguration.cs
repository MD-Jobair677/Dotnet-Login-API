using BulkMail.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulkMail.Infrastructure.Persistence.Configurations.User
{
    public class UserConfiguration : IEntityTypeConfiguration<BulkMail.Domain.User.Entities.User>
    {
        public void Configure(EntityTypeBuilder<BulkMail.Domain.User.Entities.User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(u => u.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.UserAsset)
                .WithOne(a => a.User)
                .HasForeignKey<UserAsset>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
