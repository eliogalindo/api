using System.ComponentModel.DataAnnotations;
using api.Core.Interfaces.Properties;
using api.Modules.Notifications.Enums;

namespace api.Modules.Notifications.Models;

public class Notification : IIdentifier<int>
{
    [MaxLength(250)] public string LocalizationKey { get; set; } = string.Empty;
    [MaxLength(50)] public string? LocalizationArgs { get; set; }
    [MaxLength(50)] public NotificationType Type { get; set; } = NotificationType.Default;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<UserNotification> Users { get; set; } = [];
    public int Id { get; set; }
}