
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmsSystem.Infrastructure.Persistence;
using EmsSystem.Application.DTOs;
using EmsSystem.Infrastructure.Authorization;
using EmsSystem.Common.ResponseDtos;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PermissionsController(AppDbContext context)
    {
        _context = context;
    }

    // =========================
    // GET ALL PERMISSIONS
    // =========================
    [HttpGet]
    [Authorize]
    [Permission("Role.View")]
    public async Task<IActionResult> GetPermissions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Permissions.OrderBy(p => p.Name);

        var (permissions, meta) = await query.ToPaginatedListAsync(page, pageSize);

        var result = permissions.Select(p => new PermissionResponseDto
        {
            Id = p.Id,
            Name = p.Name
        }).ToList();

        return Ok(ApiResponse<List<PermissionResponseDto>>.SuccessResponse(result, meta));
    }
}
