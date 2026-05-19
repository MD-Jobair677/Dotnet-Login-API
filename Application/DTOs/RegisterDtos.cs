using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Application.DTOs
{


    public class ResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string token { get; set; }
    }
    public class UpdateUserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Avatar { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

    public class UserAssetDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string AssetName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AssetType { get; set; } = string.Empty;

        [Required]
        public string Path { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }



    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}