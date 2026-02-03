using api.Modules.Permissions.DTOs;

namespace api.Modules.Roles.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Denomination { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PermissionDto> Permissions { get; set; } = [];
}