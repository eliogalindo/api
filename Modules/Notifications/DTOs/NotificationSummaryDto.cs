namespace api.Modules.Notifications.DTOs;

public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = [];
}