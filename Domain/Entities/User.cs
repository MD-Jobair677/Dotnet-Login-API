using LoginSystem.Domain.Entities;
namespace LoginSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string? FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserRole> UserRoles { get; set; }
        // Assets

        // Profile
        public UserProfile? UserProfile { get; set; }
        public UserAsset? UserAsset { get; set; }

    }
}