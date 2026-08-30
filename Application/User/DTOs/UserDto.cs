using System.ComponentModel.DataAnnotations;


namespace BulkMail.Application.DTOs
{

    public class UserResponseDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();

        public List<string> Permissions { get; set; } = new();
    }

    public class UserAuthResponseDto
    {
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public List<string> UserRoles { get; set; } = new();
        public List<string> UserPermissions { get; set; } = new();
        public string Token { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public List<int>? RoleIds { get; set; }
    }

    public class UpdateUserDto
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public List<int>? RoleIds { get; set; }
    }

    public class UpdateUserRolesDto
    {
        public List<int> RoleIds { get; set; } = new();
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
{
    public string Token { get; set; }

    public string NewPassword { get; set; }
}
}
