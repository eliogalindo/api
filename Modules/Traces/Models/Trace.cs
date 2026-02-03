using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Traces.Enums;
using api.Modules.Users.Models;

namespace api.Modules.Traces.Models;

public class Trace : IIdentifier<int>, ITrackable, ISoftDeletable
{
    [MaxLength(100)] public string LocalizationKey { get; set; } = string.Empty;
    [MaxLength(250)] public string? LocalizationArgs { get; set; }
    public TraceAction Action { get; set; } = TraceAction.Create;
    public int UserId { get; set; }
    [MaxLength(20)] public string? Ip { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public int Id { get; set; }
    public bool Deletable { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}