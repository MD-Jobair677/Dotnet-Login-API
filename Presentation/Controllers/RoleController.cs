using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmsSystem.Infrastructure.Persistence;
using EmsSystem.Domain.Entities;
using EmsSystem.Application.DTOs;
using EmsSystem.Infrastructure.Authorization;
using EmsSystem.Common.ResponseDtos;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // GET ALL ROLES
    // =========================
    [HttpGet]
    [Authorize]
    [Permission("Role.View")]
    public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name);

        var (roles, meta) = await query.ToPaginatedListAsync(page, pageSize);

        var result = roles.Select(role => new RoleListResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,

            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        }).ToList();

        return Ok(ApiResponse<List<RoleListResponseDto>>.SuccessResponse(result, meta));
    }

    // =========================
    //  GET ROLE BY ID
    // =========================
    [HttpGet("{id}")]
    [Authorize]
    [Permission("Role.View")]
    public async Task<IActionResult> GetRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

        var result = new RoleDetailResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,

            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return Ok(ApiResponse<RoleDetailResponseDto>.SuccessResponse(result));
    }

    // =========================
    //  CREATE ROLE
    // =========================
    [HttpPost]
    [Authorize]
    [Permission("Role.Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<object>.FailResponse("Role name required"));

        var exists = await _context.Roles
            .AnyAsync(x => x.Name == dto.Name);

        if (exists)
            return BadRequest(ApiResponse<object>.FailResponse("Role already exists"));

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var result = new RoleResponseDto
        {
            Name = role.Name,
            Description = role.Description,
            Permissions = new List<string>()
        };

        return Ok(ApiResponse<RoleResponseDto>.SuccessResponse(result, "Role created successfully"));
    }

    // =========================
    //  UPDATE ROLE
    // =========================
    [HttpPut("{id}")]
    [Authorize]
    [Permission("Role.Update")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<object>.FailResponse("Role name required"));

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

        var exists = await _context.Roles
            .AnyAsync(x =>
                x.Name.ToLower().Trim() == dto.Name.ToLower().Trim()
                && x.Id != id);

        if (exists)
            return BadRequest(ApiResponse<object>.FailResponse("Role already exists"));

        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            role.Id,
            role.Name,
            role.Description
        }, "Role updated successfully"));
    }

    // =========================
    //  GET ROLE PERMISSION IDS
    // =========================
    [HttpGet("{id}/permissions")]
    [Authorize]
    [Permission("Role.View")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == id);

        if (!roleExists)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return Ok(ApiResponse<List<int>>.SuccessResponse(permissionIds));
    }

    // =========================
    //  REPLACE ROLE PERMISSIONS
    // =========================
    [HttpPut("{id}/permissions")]
    [Authorize]
    [Permission("Role.Update")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

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
            return BadRequest(ApiResponse<object>.FailResponse("Invalid permission ids", missingPermissionIds.Select(x => x.ToString()).ToList()));

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

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            RoleId = id,
            PermissionIds = permissionIds
        }, "Role permissions updated successfully"));
    }

    // =========================
    //  ADD SINGLE ROLE PERMISSION
    // =========================
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [Authorize]
    [Permission("Role.Update")]
    public async Task<IActionResult> AddRolePermission(int roleId, int permissionId)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

        if (!roleExists)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

        var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);

        if (!permissionExists)
            return NotFound(ApiResponse<object>.FailResponse("Permission not found"));

        var alreadyAssigned = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (alreadyAssigned)
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Permission already assigned to role"));

        _context.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Permission assigned to role successfully"));
    }

    // =========================
    //  DELETE ROLE
    // =========================
    [HttpDelete("{id}")]
    [Authorize]
    [Permission("Role.Delete")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return NotFound(ApiResponse<object>.FailResponse("Role not found"));

        //  OPTIONAL SAFETY CHECK (recommended)
        if (role.UserRoles.Any())
        {
            return BadRequest(ApiResponse<object>.FailResponse("Cannot delete role. Users are assigned to this role."));
        }

        //  Step 1: remove permissions first
        if (role.RolePermissions.Any())
        {
            _context.RolePermissions.RemoveRange(role.RolePermissions);
        }

        //  Step 2: remove role
        _context.Roles.Remove(role);

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Role deleted successfully"));
    }
}
