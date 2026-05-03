

using LoginSystem.Data;
using LoginSystem.DTO;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult CreateStudent(CreateStudentDto dto)
        {
            var exists = _context.Students.Any(x => x.Email == dto.Email);
            if (exists)
            {
                return BadRequest("Email already exists");
            }
            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
            };
            _context.Students.Add(student);
            _context.SaveChanges();
            return Ok(new
            {
                success = true,
                message = "Student created successfully",
                data = student
            });


        }

        [HttpGet("{id}")]
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
        public IActionResult UpdateStudent(int id, UpdateStudentDto dto)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == id);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Phone = dto.Phone;
            student.Address = dto.Address;

            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Student updated successfully",
                data = student
            });
        }

    }

}