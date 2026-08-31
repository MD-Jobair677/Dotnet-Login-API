using BulkMail.Application.DTOs;
using EmsSystem.Common.ResponseDtos;

public interface IRoleService
{
    Task<ApiResponse<List<RoleListResponseDto>>> GetRolesAsync();
    Task<ApiResponse<RoleDetailResponseDto>> GetRoleAsync(int id);
    Task<ApiResponse<RoleResponseDto>> CreateRoleAsync(CreateRoleDto dto);
    Task<ApiResponse<RoleResponseDto>> UpdateRoleAsync(int id, UpdateRoleDto dto);
    Task<ApiResponse<List<int>>> GetRolePermissionsAsync(int id);
    Task<ApiResponse<object>> UpdateRolePermissionsAsync(int id, UpdateRolePermissionsDto dto);
    Task<ApiResponse<object>> AddRolePermissionAsync(int roleId, int permissionId);
    Task<ApiResponse<object>> DeleteRoleAsync(int id);
}
