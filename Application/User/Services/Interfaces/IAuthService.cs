using BulkMail.Application.DTOs;
using BulkMail.Common.ResponseDtos;

public interface IAuthService
{
    Task<ApiResponse<UserAuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<UserAuthResponseDto>> RegisterAsync(RegisterDto dto);
}
