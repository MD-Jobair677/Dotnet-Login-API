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
        private readonly IAuthService _authService;
        public AuthController(AppDbContext context, IConfiguration config, IImageNameService imageNameService, IAuthService authService)
        {
            _config = config;
            _context = context;
            _imageNameService = imageNameService;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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