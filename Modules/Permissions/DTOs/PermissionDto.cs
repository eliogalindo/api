using api.Modules.Permissions.Enums;

namespace api.Modules.Permissions.DTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public PermissionGroup Group { get; set; }
    public PermissionAction Action { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PermissionTranslationDto> Translations { get; set; } = [];
}