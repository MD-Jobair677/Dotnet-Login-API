









using LoginSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginSystem.Application.DTOs;
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
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _context.Permissions.ToListAsync();

        var result = permissions.Select(p => new PermissionResponseDto
        {
            Id = p.Id,
            Name = p.Name
        }).ToList();

        return Ok(result);
    }
}