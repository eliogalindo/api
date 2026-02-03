using System.ComponentModel.DataAnnotations;

namespace api.Modules.Permissions.DTOs;

public class DeletePermissionsDto
{
    [Required] [MinLength(1)] public int[] PermissionIds { get; set; } = [];
}