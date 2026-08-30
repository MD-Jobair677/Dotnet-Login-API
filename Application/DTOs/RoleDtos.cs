

using System.ComponentModel.DataAnnotations;
using BulkMail.Application.DTOs;


namespace BulkMail.Application.DTOs
{
    public class RoleResponseDto
    {

        public required string Name { get; set; }

        public string? Description { get; set; }

        public List<string> Permissions { get; set; } = new();



    }

    public class RoleDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<string> Permissions { get; set; } = new();
    }
    public class RoleListResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<string> Permissions { get; set; } = new();
    }
    public class CreateRoleDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
    public class UpdateRoleDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateRolePermissionsDto
    {
        public List<int> PermissionIds { get; set; } = new();
    }





}
