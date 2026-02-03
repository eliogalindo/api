using System.ComponentModel.DataAnnotations;

namespace api.Modules.Roles.DTOs;

public class CreateRoleDto
{
    [Required] [MaxLength(100)] public string Denomination { get; set; } = string.Empty;

    [Required] [MaxLength(150)] public string Description { get; set; } = string.Empty;

    [Required] public bool Enabled { get; set; }

    [Required] [MinLength(1)] public List<int> Permissions { get; set; } = [];
}