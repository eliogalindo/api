using System.ComponentModel.DataAnnotations;

namespace api.Modules.Roles.DTOs;

public class DeleteRolesDto
{
    [Required] [MinLength(1)] public int[] RoleIds { get; set; } = [];
}