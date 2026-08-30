using System.ComponentModel.DataAnnotations;

namespace BulkMail.Application.DTOs
{


    public class ResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; }
    }
    
    public class RegisterResponseDto
    {
        required
        public string FirstName { get; set; }
        required
        public string LastName { get; set; }
        public string Email { get; set; }
        public string token { get; set; }
    }
    public class UpdateUserProfileDto
    {
        public string? FirstName { get; set; }
        public string?LastName { get; set; }

     

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Avatar { get; set; }

        public string? Bio { get; set; }



    }

    public class UserAssetDto
    {
        
        
        public IFormFile? Path { get; set; } = null!;
    }



    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

}