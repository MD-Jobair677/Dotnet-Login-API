using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginSystem.Domain.Entities;
using LoginSystem.Application.DTOs;
using LoginSystem.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace LoginSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);

            if (user == null)
                return Unauthorized("User not found");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValidPassword)
                return Unauthorized("Wrong password");

            var token = GenerateToken(user.FirstName, user.LastName, user.Email, user);

            var response = new ResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Data = new
                {
                    userFirstName = user.FirstName,
                    userLastName = user.LastName,
                    userEmail = user.Email,


                    token = token
                }
            };

            return Ok(response);
        }

        private string GenerateToken(string userFirstName, string userLastName, string userEmail, User user)
        {
            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("Jwt:Key configuration is missing.");
            }
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)),
                new Claim(ClaimTypes.Name, userFirstName),
                new Claim(ClaimTypes.Surname, userLastName),
                new Claim(ClaimTypes.Email, userEmail)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        [HttpPost("register")]
        public IActionResult RegisterUser(RegisterDto dto)
        {
            // check email exists
            var userExists = _context.Users.FirstOrDefault(x => x.Email == dto.Email);
            if (userExists != null)
            {
                return BadRequest("Email already exists");
            }

            // create user
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            var token = GenerateToken(user.FirstName, user.LastName, user.Email, user);



            var response = new ResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Data = new RegisterDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    token = token
                }
            };

            return Ok(response);
        }

      [Authorize]
[HttpPost("user-profile-update")]
public IActionResult UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (userId == null)
        return Unauthorized();

    var user = _context.Users
        .Include(u => u.UserProfile)
        .Include(u => u.UserAsset) // one-to-one
        .FirstOrDefault(x => x.Id == int.Parse(userId));

    if (user == null)
        return NotFound("User not found");

    // =====================
    // USER UPDATE
    // =====================
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;

    // =====================
    // PROFILE UPSERT
    // =====================
    if (user.UserProfile == null)
    {
        user.UserProfile = new UserProfile
        {
            UserId = user.Id,
            Phone = dto.Phone,
            Address = dto.Address,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Avatar = dto.Avatar,
            Bio = dto.Bio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    else
    {
        user.UserProfile.Phone = dto.Phone;
        user.UserProfile.Address = dto.Address;
        user.UserProfile.Gender = dto.Gender;
        user.UserProfile.DateOfBirth = dto.DateOfBirth;
        user.UserProfile.Avatar = dto.Avatar;
        user.UserProfile.Bio = dto.Bio;
        user.UserProfile.UpdatedAt = DateTime.UtcNow;
    }

    _context.SaveChanges();

    // =====================
    // USER ASSET DTO (single)
    // =====================
    var asset = user.UserAsset == null ? null : new UserAssetDto
    {
        UserId = user.UserAsset.UserId,
        AssetName = user.UserAsset.AssetName,
        AssetType = user.UserAsset.AssetType,
        Path = user.UserAsset.Path,
        UpdatedAt = user.UserAsset.UpdatedAt
    };

    return Ok(new
    {
        message = "User profile saved successfully",

        user = new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email
        },

        profile = user.UserProfile,

        asset = asset
    });
}
    };


}