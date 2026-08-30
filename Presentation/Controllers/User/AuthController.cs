using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using BulkMail.Domain.User.Entities;
using BulkMail.Application.DTOs;
using BulkMail.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BulkMail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IImageNameService _imageNameService;
        public AuthController(AppDbContext context, IConfiguration config, IImageNameService imageNameService)
        {
            _config = config;
            _context = context;
            _imageNameService = imageNameService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                         .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                         .FirstOrDefault(x => x.Email == request.Email);
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
                    userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    userPermissions = user.UserRoles
                        .SelectMany(ur => ur.Role.RolePermissions)
                        .Select(rp => rp.Permission.Name)
                        .Distinct()
                        .ToList(),
                    token = token
                }
            };

            return Ok(response);
        }

private string GenerateToken(
       string userFirstName,
       string userLastName,
       string userEmail,
       User user)
        {
            var keyString = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException(
                    "Jwt:Key configuration is missing."
                );
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(userFirstName))
                claims.Add(new Claim(ClaimTypes.Name, userFirstName));

            if (!string.IsNullOrEmpty(userLastName))
                claims.Add(new Claim(ClaimTypes.Surname, userLastName));

            if (!string.IsNullOrEmpty(userEmail))
                claims.Add(new Claim(ClaimTypes.Email, userEmail));

            if (user?.Id > 0)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)));

            // ROLE + PERMISSION CLAIMS
            if (user?.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole?.Role?.Name != null)
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

                    if (userRole?.Role?.RolePermissions != null)
                    {
                        foreach (var rolePermission in userRole.Role.RolePermissions)
                        {
                            if (rolePermission?.Permission?.Name != null)
                                claims.Add(new Claim("Permission", rolePermission.Permission.Name));
                        }
                    }
                }
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
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
            user.UserRoles = new List<UserRole>(); // Initialize empty roles
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
                    userRoles = new List<string>(),
                    userPermissions = new List<string>(),
                    token = token
                }
            };

            return Ok(response);
        }

        // [Authorize]
        [HttpPut("user-profile-update")]
        public IActionResult UpdateUserProfile(
    [FromForm] UpdateUserProfileDto dto,
    [FromForm] UserAssetDto userAssetDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _context.Users
                .Include(u => u.UserProfile)
                .Include(u => u.UserAsset)
                .FirstOrDefault(x => x.Id == int.Parse(userId));

            if (user == null)
                return NotFound("User not found");

            // ===================== USER UPDATE =====================
            if (!string.IsNullOrEmpty(dto.FirstName))
            {
                return BadRequest("First name cannot be empty");
            }if (!string.IsNullOrEmpty(dto.LastName))
            {
                return BadRequest("Last name cannot be empty");
            }
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            // ===================== PROFILE UPSERT =====================
            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile
                {
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth,
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
                user.UserProfile.Bio = dto.Bio;
                user.UserProfile.UpdatedAt = DateTime.UtcNow;
            }

            // ===================== IMAGE UPLOAD =====================
            string? imageName = null;
            string? imagePath = null;
            string? imageType = null;

            if (userAssetDto.Path != null)
            {



                // ===================== DELETE OLD IMAGE =====================
                if (user.UserAsset != null && !string.IsNullOrEmpty(user.UserAsset.Path))
                {
                    FileUploadHelper.DeleteImage(user.UserAsset.Path);
                }
                // generate image name
                imageName = _imageNameService.GenerateImageName(userAssetDto.Path.FileName);

                // upload file
                imagePath = FileUploadHelper.UploadImage(
                    userAssetDto.Path,
                    "user",
                    imageName,
                     new[] { ".jpg", ".png", ".jpeg", ".webp" }, 2
                );

                // get extension
                imageType = _imageNameService.GetImageExtension(userAssetDto.Path.FileName);
            }

            // ===================== USER ASSET UPSERT =====================
            if (imagePath != null)
            {
                if (user.UserAsset == null)
                {
                    user.UserAsset = new UserAsset
                    {
                        AssetName = imageName,
                        AssetType = imageType,
                        Path = imagePath,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    user.UserAsset.AssetName = imageName;
                    user.UserAsset.AssetType = imageType;
                    user.UserAsset.Path = imagePath;
                    user.UserAsset.UpdatedAt = DateTime.UtcNow;
                }
            }

            // ===================== SAVE =====================
            _context.SaveChanges();

            // ===================== RESPONSE =====================
            var asset = user.UserAsset == null
                ? null
                : new
                {
                    user.UserAsset.AssetName,
                    user.UserAsset.AssetType,
                    user.UserAsset.Path,
                    user.UserAsset.UpdatedAt
                };

            var profile = user.UserProfile == null
                ? null
                : new
                {
                    user.UserProfile.Phone,
                    user.UserProfile.Address,
                    user.UserProfile.Gender,
                    user.UserProfile.DateOfBirth,
                    user.UserProfile.Bio
                };

            return Ok(new
            {
                message = "User profile updated successfully",

                user = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email
                },

                profile,
                asset
            });
        }

    };


}