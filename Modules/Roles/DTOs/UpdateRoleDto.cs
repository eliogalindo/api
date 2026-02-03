using System.ComponentModel.DataAnnotations;

namespace api.Modules.Roles.DTOs;

public class UpdateRoleDto
{
    [Required] [MaxLength(50)] public string Denomination { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    [Required] [MinLength(1)] public List<int> Permissions { get; set; } = [];
}