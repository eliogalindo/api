namespace api.Modules.Notifications.Models;

public class NotificationQueueMessage(int userId, string culture)
{
    public int UserId { get; set; } = userId;
    public string Culture { get; set; } = culture;
}