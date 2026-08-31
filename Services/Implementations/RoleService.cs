using BulkMail.Application.DTOs;
using BulkMail.Domain.User.Entities;
using BulkMail.Infrastructure.Persistence;
using EmsSystem.Common.ResponseDtos;
using Microsoft.EntityFrameworkCore;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<RoleListResponseDto>>> GetRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var result = roles.Select(role => new RoleListResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        }).ToList();

        return ApiResponse<List<RoleListResponseDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<RoleDetailResponseDto>> GetRoleAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return ApiResponse<RoleDetailResponseDto>.FailResponse("Role not found");

        var result = new RoleDetailResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return ApiResponse<RoleDetailResponseDto>.SuccessResponse(result);
    }

    public async Task<ApiResponse<RoleResponseDto>> CreateRoleAsync(CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<RoleResponseDto>.FailResponse("Role name required");

        var exists = await _context.Roles
            .AnyAsync(x => x.Name == dto.Name);

        if (exists)
            return ApiResponse<RoleResponseDto>.FailResponse("Role already exists");

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var createdRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == role.Id);

        var result = new RoleResponseDto
        {
            Name = createdRole.Name,
            Description = createdRole.Description,
            Permissions = createdRole.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return ApiResponse<RoleResponseDto>.SuccessResponse(result, "Role created successfully");
    }

    public async Task<ApiResponse<RoleResponseDto>> UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<RoleResponseDto>.FailResponse("Role name required");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return ApiResponse<RoleResponseDto>.FailResponse("Role not found");

        var exists = await _context.Roles
            .AnyAsync(x =>
                x.Name.ToLower().Trim() == dto.Name.ToLower().Trim()
                && x.Id != id);

        if (exists)
            return ApiResponse<RoleResponseDto>.FailResponse("Role already exists");

        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();

        var result = new RoleResponseDto
        {
            Name = role.Name,
            Description = role.Description,
            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return ApiResponse<RoleResponseDto>.SuccessResponse(result, "Role updated successfully");
    }

    public async Task<ApiResponse<List<int>>> GetRolePermissionsAsync(int id)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == id);

        if (!roleExists)
            return ApiResponse<List<int>>.FailResponse("Role not found");

        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return ApiResponse<List<int>>.SuccessResponse(permissionIds);
    }

    public async Task<ApiResponse<object>> UpdateRolePermissionsAsync(int id, UpdateRolePermissionsDto dto)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return ApiResponse<object>.FailResponse("Role not found");

        var permissionIds = dto.PermissionIds
            .Distinct()
            .ToList();

        var existingPermissionIds = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var missingPermissionIds = permissionIds
            .Except(existingPermissionIds)
            .ToList();

        if (missingPermissionIds.Any())
            return ApiResponse<object>.FailResponse(
                "Invalid permission ids",
                missingPermissionIds.Select(id => $"Permission id {id} does not exist").ToList());

        var currentPermissionIds = role.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToList();

        var permissionsToRemove = role.RolePermissions
            .Where(rp => !permissionIds.Contains(rp.PermissionId))
            .ToList();

        var permissionIdsToAdd = permissionIds
            .Except(currentPermissionIds)
            .ToList();

        if (permissionsToRemove.Any())
        {
            _context.RolePermissions.RemoveRange(permissionsToRemove);
        }

        foreach (var permissionId in permissionIdsToAdd)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                PermissionId = permissionId
            });
        }

        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResponse(
            new { RoleId = id, PermissionIds = permissionIds },
            "Role permissions updated successfully");
    }

    public async Task<ApiResponse<object>> AddRolePermissionAsync(int roleId, int permissionId)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

        if (!roleExists)
            return ApiResponse<object>.FailResponse("Role not found");

        var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);

        if (!permissionExists)
            return ApiResponse<object>.FailResponse("Permission not found");

        var alreadyAssigned = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (alreadyAssigned)
            return ApiResponse<object>.FailResponse("Permission already assigned to role");

        _context.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResponse(null, "Permission assigned to role successfully");
    }

    public async Task<ApiResponse<object>> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return ApiResponse<object>.FailResponse("Role not found");

        if (role.UserRoles.Any())
            return ApiResponse<object>.FailResponse("Cannot delete role. Users are assigned to this role.");

        if (role.RolePermissions.Any())
        {
            _context.RolePermissions.RemoveRange(role.RolePermissions);
        }

        _context.Roles.Remove(role);

        await _context.SaveChangesAsync();

        return ApiResponse<object>.SuccessResponse(null, "Role deleted successfully");
    }
}
