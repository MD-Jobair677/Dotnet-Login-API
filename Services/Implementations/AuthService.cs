using BulkMail.Application.DTOs;
using BulkMail.Domain.User.Entities;
using BulkMail.Infrastructure.Persistence;
using EmsSystem.Common.ResponseDtos;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<ApiResponse<UserAuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
            return ApiResponse<UserAuthResponseDto>.FailResponse("User not found");

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!isValidPassword)
            return ApiResponse<UserAuthResponseDto>.FailResponse("Wrong password");

        var token = GenerateToken(user);

        return ApiResponse<UserAuthResponseDto>.SuccessResponse(
            BuildUserData(user, token),
            "Login successful");
    }

    public async Task<ApiResponse<UserAuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var userExists = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (userExists != null)
            return ApiResponse<UserAuthResponseDto>.FailResponse("Email already exists");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.UserRoles = new List<UserRole>();
        var token = GenerateToken(user);

        return ApiResponse<UserAuthResponseDto>.SuccessResponse(
            BuildUserData(user, token),
            "User registered successfully");
    }

    private string GenerateToken(User user)
    {
        var keyString = _config["Jwt:Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("Jwt:Key configuration is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(ClaimTypes.Name, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        if (user.Id > 0)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)));

        if (user.UserRoles != null)
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

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserAuthResponseDto BuildUserData(User user, string token)
    {
        return new UserAuthResponseDto
        {
            UserFirstName = user.FirstName,
            UserLastName = user.LastName,
            UserEmail = user.Email,
            UserRoles = user.UserRoles?
                .Select(ur => ur.Role?.Name)
                .Where(name => name != null)
                .ToList() ?? new List<string>(),
            UserPermissions = user.UserRoles?
                .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
                .Select(rp => rp.Permission?.Name)
                .Where(name => name != null)
                .Distinct()
                .ToList() ?? new List<string>(),
            Token = token
        };
    }
}
