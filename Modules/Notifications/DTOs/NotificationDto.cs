using api.Modules.Notifications.Enums;

namespace api.Modules.Notifications.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Default;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}