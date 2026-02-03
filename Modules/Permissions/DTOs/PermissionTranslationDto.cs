namespace api.Modules.Permissions.DTOs;

public class PermissionTranslationDto
{
    public int Id { get; set; }
    public int PermissionId { get; set; }
    public string Locale { get; set; } = string.Empty;
    public string Denomination { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}