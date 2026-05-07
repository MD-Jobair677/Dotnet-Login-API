

using LoginSystem.Infrastructure.Persistence;
using LoginSystem.Application.DTOs;
using LoginSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LoginSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {

        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        [Authorize]

        public IActionResult GetAllStudents()
        {
            var students = _context.Students.ToList();

            return Ok(new
            {
                success = true,
                message = "All students fetched",
                data = students
            });
        }

        [HttpPost("create/student")]
        public IActionResult CreateStudent([FromForm] CreateStudentDto dto)
        {
            var exists = _context.Students.Any(x => x.Email == dto.Email);
            if (exists)
            {
                return BadRequest("Email already exists");
            }
            var imagePath = FileUploadHelper.UploadImage(dto.ProfileImage, "students");

            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Profile = new StudentProfile
                {
                    ProfileImage = imagePath
                }

            };





            _context.Students.Add(student);
            _context.SaveChanges();
            return Ok(new
            {
                success = true,
                message = "Student created successfully",
                data = new StudentResponseDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    Profile = new StudentProfileDto
                    {
                        Id = student.Profile.Id,
                        ProfileImage = student.Profile.ProfileImage
                    }
                }
            });


        }

        [HttpGet("{id}")]
        [Authorize]

        public IActionResult GetStudentById(int id)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == id);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            return Ok(new
            {
                success = true,
                data = student
            });
        }


        [HttpPut("update/{id}")]

        public IActionResult UpdateStudent([FromForm] int id, [FromForm] UpdateStudentDto dto)
        {
            var student = _context.Students
                .Include(x => x.Profile)
                .FirstOrDefault(x => x.Id == id);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            // -------------------
            // Student update
            // -------------------
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Phone = dto.Phone;
            student.Address = dto.Address;

            // -------------------
            // PROFILE IMAGE LOGIC
            // -------------------
            if (dto.ProfileImage != null)
            {
                var newImagePath = FileUploadHelper.UploadImage(dto.ProfileImage, "students");

                // delete old image
                if (student.Profile != null)
                {
                    FileUploadHelper.DeleteImage(student.Profile.ProfileImage);
                }

                // update or create profile
                if (student.Profile != null)
                {
                    student.Profile.ProfileImage = newImagePath;
                }
                else
                {
                    student.Profile = new StudentProfile
                    {
                        ProfileImage = newImagePath
                    };
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Student created successfully",
                data = new StudentResponseDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    Profile = new StudentProfileDto
                    {
                        Id = student.Profile.Id,
                        ProfileImage = student.Profile.ProfileImage
                    }
                }
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students
                .Include(x => x.Profile)
                .FirstOrDefault(x => x.Id == id);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            // delete profile image if exists
            if (student.Profile != null && !string.IsNullOrWhiteSpace(student.Profile.ProfileImage))
            {
                FileUploadHelper.DeleteImage(student.Profile.ProfileImage);
            }

            _context.Students.Remove(student);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Student deleted successfully"
            });
        }

    }

}
