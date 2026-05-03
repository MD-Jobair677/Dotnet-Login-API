

using LoginSystem.Data;
using LoginSystem.DTO;
using LoginSystem.Models;
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

        [Route("create/student")]
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
                Phone = dto.Phone
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

    }

}