
using BulkMail.Infrastructure.Persistence;
using BulkMail.Application.DTOs;
using EmsSystem.Common.ResponseDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> GetPermissions([FromQuery] PaginationQuery query)
    {
        var queryable = _context.Permissions.AsQueryable();

        var (permissions, meta) = await queryable.ToPaginatedListAsync(query.Page, query.PageSize);

        var result = permissions.Select(p => new PermissionResponseDto
        {
            Id = p.Id,
            Name = p.Name
        }).ToList();

        return Ok(ApiResponse<List<PermissionResponseDto>>.SuccessResponse(result, meta));
    }
}