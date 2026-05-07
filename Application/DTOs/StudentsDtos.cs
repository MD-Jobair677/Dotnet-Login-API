
using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Application.DTOs
{
    public class StudentResponseDto
    {
        public int Id { get; set; }
        public required  string FirstName { get; set; }
        public required  string  LastName { get; set; }
        public  required  string Email { get; set; }

        public StudentProfileDto Profile { get; set; }
    }
    public class CreateStudentDto
    {
        [Required]
        public required  string FirstName { get; set; }

        [Required]
        public required  string LastName { get; set; }

        [Required]
        public required  string Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public IFormFile? ProfileImage { get; set; }

    }

    public class UpdateStudentDto
    {
        public required  string FirstName { get; set; }
        public required  string LastName { get; set; }
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