    using System.ComponentModel.DataAnnotations;

    namespace LoginSystem.Models
    {
        public class Student
        {
            public int Id { get; set; }
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            public string Email { get; set; }
            public string? Phone { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string? Address { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }