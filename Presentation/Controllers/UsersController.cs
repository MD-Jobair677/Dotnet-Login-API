using EmsSystem.Application.DTOs;
using EmsSystem.Domain.Entities;
using EmsSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmsSystem.Infrastructure.Authorization;
using EmsSystem.Common.ResponseDtos;

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
    [Authorize]
    [Permission("User.View")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .OrderByDescending(u => u.CreatedAt);

        var (users, meta) = await query.ToPaginatedListAsync(page, pageSize);

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

        return Ok(ApiResponse<List<UserResponseDto>>.SuccessResponse(result, meta));
    }

    [HttpGet("{id}/roles")]
    [Authorize]
    [Permission("User.View")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == id);

        if (!userExists)
            return NotFound(ApiResponse<object>.FailResponse("User not found"));

        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        return Ok(ApiResponse<List<int>>.SuccessResponse(roleIds));
    }

    [HttpPut("{id}/roles")]
    [Authorize]
    [Permission("User.Update")]
    public async Task<IActionResult> UpdateUserRoles(int id, [FromBody] UpdateUserRolesDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(ApiResponse<object>.FailResponse("User not found"));

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
            return BadRequest(ApiResponse<object>.FailResponse("Invalid role ids", missingRoleIds.Select(x => x.ToString()).ToList()));

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

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            UserId = id,
            RoleIds = roleIds
        }, "User roles updated successfully"));
    }
}
