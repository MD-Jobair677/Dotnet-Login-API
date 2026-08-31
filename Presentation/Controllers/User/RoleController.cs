using Microsoft.AspNetCore.Mvc;
using BulkMail.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _roleService.GetRolesAsync();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(int id)
    {
        var result = await _roleService.GetRoleAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateRoleAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _roleService.UpdateRoleAsync(id, dto);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var result = await _roleService.GetRolePermissionsAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var result = await _roleService.UpdateRolePermissionsAsync(id, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AddRolePermission(int roleId, int permissionId)
    {
        var result = await _roleService.AddRolePermissionAsync(roleId, permissionId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
