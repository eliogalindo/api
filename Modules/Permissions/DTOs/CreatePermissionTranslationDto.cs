using System.ComponentModel.DataAnnotations;

namespace api.Modules.Permissions.DTOs;

public abstract class CreatePermissionTranslationDto
{
    [Required] [MaxLength(10)] public string Locale { get; set; } = string.Empty;
    [Required] [MaxLength(100)] public string Denomination { get; set; } = string.Empty;
    [Required] [MaxLength(100)] public string Description { get; set; } = string.Empty;
}