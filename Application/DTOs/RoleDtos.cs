

using System.ComponentModel.DataAnnotations;
using LoginSystem.Application.DTOs;


namespace LoginSystem.Application.DTOs
{
    public class RoleResponseDto
    {

        public required string Name { get; set; }


        public List<string> Permissions { get; set; } = new();



    }

    public class RoleDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
    }
    public class RoleListResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
    }
    public class CreateRoleDto
    {
        public required string Name { get; set; }
    }
    public class UpdateRoleDto
    {
        public required string Name { get; set; }
    }





}