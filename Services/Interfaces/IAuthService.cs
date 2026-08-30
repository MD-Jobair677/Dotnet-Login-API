using BulkMail.Application.DTOs;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> RegisterAsync(RegisterDto dto);
}
