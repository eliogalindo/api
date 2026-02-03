using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;

namespace api.Modules.Permissions.Models;

public class PermissionTranslation : IIdentifier<int>
{
    public int PermissionId { get; set; }
    [MaxLength(10)] public string Locale { get; set; } = string.Empty; // Eg: "es-ES", "en-US"
    [MaxLength(100)] public string Denomination { get; set; } = string.Empty;
    [MaxLength(100)] public string Description { get; set; } = string.Empty;
    public Permission Permission { get; set; } = null!;
    public int Id { get; set; }
}