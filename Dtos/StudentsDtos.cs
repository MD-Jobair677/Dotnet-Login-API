
using System.ComponentModel.DataAnnotations;

namespace LoginSystem.DTO
{
    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }

        public StudentProfileDto Profile { get; set; }
    }
    public class CreateStudentDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public IFormFile? ProfileImage { get; set; }

    }

    public class UpdateStudentDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public IFormFile? ProfileImage { get; set; }

        public string? Address
        {
            get; set;
        }
    }

    public class StudentProfileDto
    {
        public int Id { get; set; }
        public string? ProfileImage { get; set; }
    }


}