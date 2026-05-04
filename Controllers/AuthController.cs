using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginSystem.Models;
using LoginSystem.DTO;
using LoginSystem.Data;
using BCrypt.Net;

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

            var token = GenerateToken(user.Email, user.LastName, user.LastName);

            var response = new ResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Data = new
                {
                    email = user.Email,
                    token = token
                }
            };

            return Ok(response);
        }

        private string GenerateToken(string useFirstName, string useLastName, string userEmail)
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
                new Claim(ClaimTypes.Name, useFirstName),
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
            var token = GenerateToken(user.Email, user.LastName, user.LastName);



            var response = new ResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Data = new
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    email = user.Email,
                    token = token
                }
            };

            return Ok(response);
        }

    };



}