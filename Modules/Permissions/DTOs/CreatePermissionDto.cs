using System.ComponentModel.DataAnnotations;
using api.Modules.Permissions.Enums;

namespace api.Modules.Permissions.DTOs;

public class CreatePermissionDto
{
    [Required] [MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required] public PermissionGroup Group { get; set; }
    [Required] public PermissionAction Action { get; set; }
    [Required] public List<CreatePermissionTranslationDto> Translations { get; set; } = [];
}