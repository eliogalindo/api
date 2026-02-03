using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Permissions.Enums;
using api.Modules.Roles.Models;

namespace api.Modules.Permissions.Models;

public class Permission : IIdentifier<int>, ITrackable, ISoftDeletable
{
    [MaxLength(50)] public string Code { get; set; } = string.Empty;
    public PermissionGroup Group { get; set; }
    public PermissionAction Action { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Role> Roles { get; set; } = [];
    public List<PermissionTranslation> Translations { get; set; } = [];
    public int Id { get; set; }
    public bool Deletable { set; get; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}