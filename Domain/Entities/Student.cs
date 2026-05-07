    using System.ComponentModel.DataAnnotations;

    namespace LoginSystem.Domain.Entities
    {
        public class Student
        {
            public int Id { get; set; }
            [Required]
            public required  string FirstName { get; set; }

            [Required]
            public required  string LastName { get; set; }

            [Required]
            public required  string Email { get; set; }
            public string? Phone { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string? Address { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public StudentProfile? Profile { get; set; }
        }
    }