using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Permissions.Models;
using api.Modules.Users.Models;

namespace api.Modules.Roles.Models;

public class Role : IIdentifier<int>, ITrackable, ISoftDeletable
{
    [MaxLength(50)] public string Denomination { get; set; } = string.Empty;
    [MaxLength(100)] public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<User> Users { get; set; } = [];
    public List<Permission> Permissions { get; set; } = [];
    public int Id { get; set; }
    public bool Deletable { set; get; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}