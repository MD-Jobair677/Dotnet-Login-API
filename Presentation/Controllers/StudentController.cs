

using EmsSystem.Infrastructure.Persistence;
using EmsSystem.Application.DTOs;
using EmsSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmsSystem.Infrastructure.Authorization;
using EmsSystem.Common.ResponseDtos;
namespace EmsSystem.Controllers
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
        [Permission("Student.View")]
        public async Task<IActionResult> GetAllStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Students
                .Include(x => x.Profile)
                .OrderByDescending(x => x.CreatedAt);

            var (students, meta) = await query.ToPaginatedListAsync(page, pageSize);

            var result = students.Select(student => new StudentResponseDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Profile = student.Profile == null ? null : new StudentProfileDto
                {
                    Id = student.Profile.Id,
                    ProfileImage = student.Profile.ProfileImage
                }
            }).ToList();

            return Ok(ApiResponse<List<StudentResponseDto>>.SuccessResponse(result, meta, "All students fetched"));
        }

        [HttpPost("create/student")]
        [Authorize]
        [Permission("Student.Create")]
        public async Task<IActionResult> CreateStudent([FromForm] CreateStudentDto dto)
        {
            var exists = await _context.Students.AnyAsync(x => x.Email == dto.Email);
            if (exists)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Email already exists"));
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
            await _context.SaveChangesAsync();

            var result = new StudentResponseDto
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
            };

            return Ok(ApiResponse<StudentResponseDto>.SuccessResponse(result, "Student created successfully"));
        }

        [HttpGet("{id}")]
        [Authorize]
        [Permission("Student.View")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students
                .Include(x => x.Profile)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.FailResponse("Student not found"));
            }

            var result = new StudentResponseDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Profile = student.Profile == null ? null : new StudentProfileDto
                {
                    Id = student.Profile.Id,
                    ProfileImage = student.Profile.ProfileImage
                }
            };

            return Ok(ApiResponse<StudentResponseDto>.SuccessResponse(result));
        }


        [HttpPut("update/{id}")]
        [Authorize]
        [Permission("Student.Update")]
        public async Task<IActionResult> UpdateStudent([FromForm] int id, [FromForm] UpdateStudentDto dto)
        {
            var student = await _context.Students
                .Include(x => x.Profile)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.FailResponse("Student not found"));
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

            await _context.SaveChangesAsync();

            var result = new StudentResponseDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Profile = student.Profile == null ? null : new StudentProfileDto
                {
                    Id = student.Profile.Id,
                    ProfileImage = student.Profile.ProfileImage
                }
            };

            return Ok(ApiResponse<StudentResponseDto>.SuccessResponse(result, "Student updated successfully"));
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        [Permission("Student.Delete")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(x => x.Profile)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (student == null)
            {
                return NotFound(ApiResponse<object>.FailResponse("Student not found"));
            }

            // delete profile image if exists
            if (student.Profile != null && !string.IsNullOrWhiteSpace(student.Profile.ProfileImage))
            {
                FileUploadHelper.DeleteImage(student.Profile.ProfileImage);
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Student deleted successfully"));
        }

    }

}
