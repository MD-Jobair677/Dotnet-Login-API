using BulkMail.Application.DTOs;
using BulkMail.Domain.Entities;
using BulkMail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var result = users.Select(user => new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList(),
            Permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .OrderBy(permission => permission)
                .ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == id);

        if (!userExists)
            return NotFound("User not found");

        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        return Ok(roleIds);
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateUserRoles(int id, [FromBody] UpdateUserRolesDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("User not found");

        var roleIds = dto.RoleIds
            .Distinct()
            .ToList();

        var existingRoleIds = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        var missingRoleIds = roleIds
            .Except(existingRoleIds)
            .ToList();

        if (missingRoleIds.Any())
            return BadRequest(new { message = "Invalid role ids", roleIds = missingRoleIds });

        var currentRoleIds = user.UserRoles
            .Select(ur => ur.RoleId)
            .ToList();

        var rolesToRemove = user.UserRoles
            .Where(ur => !roleIds.Contains(ur.RoleId))
            .ToList();

        var roleIdsToAdd = roleIds
            .Except(currentRoleIds)
            .ToList();

        if (rolesToRemove.Any())
        {
            _context.UserRoles.RemoveRange(rolesToRemove);
        }

        foreach (var roleId in roleIdsToAdd)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = id,
                RoleId = roleId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            UserId = id,
            RoleIds = roleIds
        });
    }


    
}
