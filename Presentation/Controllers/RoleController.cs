using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginSystem.Infrastructure.Persistence;
using LoginSystem.Domain.Entities;
using LoginSystem.Application.DTOs;

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
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var result = roles.Select(role => new RoleListResponseDto
        {
            Id = role.Id,
            Name = role.Name,

            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        }).ToList();

        return Ok(result);
    }

    // =========================
    //  GET ROLE BY ID
    // =========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return NotFound("Role not found");

        var result = new RoleDetailResponseDto
        {
            Id = role.Id,
            Name = role.Name,

            Permissions = role.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return Ok(result);
    }

    // =========================
    //  CREATE ROLE
    // =========================
    [HttpPost]

    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Role name required");

        var exists = await _context.Roles
            .AnyAsync(x => x.Name == dto.Name);

        if (exists)
            return BadRequest("Role already exists");

        var role = new Role
        {
            Name = dto.Name
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

            Permissions = createdRole.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList()
        };

        return Ok(result);
    }

    // =========================
    //  UPDATE ROLE
    // =========================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Role name required");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound("Role not found");

        var exists = await _context.Roles
            .AnyAsync(x =>
                x.Name.ToLower().Trim() == dto.Name.ToLower().Trim()
                && x.Id != id);

        if (exists)
            return BadRequest("Role already exists");

        role.Name = dto.Name.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            role.Id,
            role.Name
        });
    }

    // =========================
    //  GET ROLE PERMISSION IDS
    // =========================
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == id);

        if (!roleExists)
            return NotFound("Role not found");

        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        return Ok(permissionIds);
    }

    // =========================
    //  REPLACE ROLE PERMISSIONS
    // =========================
    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound("Role not found");

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
            return BadRequest(new { message = "Invalid permission ids", permissionIds = missingPermissionIds });

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

        return Ok(new
        {
            RoleId = id,
            PermissionIds = permissionIds
        });
    }

    // =========================
    //  ADD SINGLE ROLE PERMISSION
    // =========================
    [HttpPost("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AddRolePermission(int roleId, int permissionId)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

        if (!roleExists)
            return NotFound("Role not found");

        var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);

        if (!permissionExists)
            return NotFound("Permission not found");

        var alreadyAssigned = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (alreadyAssigned)
            return Ok("Permission already assigned to role");

        _context.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        await _context.SaveChangesAsync();

        return Ok("Permission assigned to role successfully");
    }

    // =========================
    //  DELETE ROLE
    // =========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role == null)
            return NotFound("Role not found");

        //  OPTIONAL SAFETY CHECK (recommended)
        if (role.UserRoles.Any())
        {
            return BadRequest("Cannot delete role. Users are assigned to this role.");
        }

        //  Step 1: remove permissions first
        if (role.RolePermissions.Any())
        {
            _context.RolePermissions.RemoveRange(role.RolePermissions);
        }

        //  Step 2: remove role
        _context.Roles.Remove(role);

        await _context.SaveChangesAsync();

        return Ok("Role deleted successfully");
    }
}
