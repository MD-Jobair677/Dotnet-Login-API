using System.ComponentModel.DataAnnotations;


namespace LoginSystem.Application.DTOs
{

    public class UserResponseDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();
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
}