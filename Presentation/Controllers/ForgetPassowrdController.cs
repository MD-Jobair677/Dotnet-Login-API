


using BulkMail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkMail.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ForgotPasswordController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        // generate token
        var token = Guid.NewGuid().ToString();

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        var resetLink =
            $"http://localhost:3000/reset-password?token={token}";

        // send mail
        await _emailService.SendEmailAsync(
            user.Email,
            "Reset Password",
            $"Click here to reset password: {resetLink}"
        );

        return Ok("Password reset link sent");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    ResetPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.PasswordResetToken == dto.Token);

        if (user == null)
        {
            return BadRequest("Invalid token");
        }

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return BadRequest("Token expired");
        }

        // password hash
        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        // clear token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _context.SaveChangesAsync();

        return Ok("Password reset successful");
    }
}