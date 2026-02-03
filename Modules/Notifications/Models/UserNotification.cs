using api.Modules.Users.Models;

namespace api.Modules.Notifications.Models;

public class UserNotification
{
    public int UserId { get; set; }
    public int NotificationId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public User User { get; set; } = null!;
    public Notification Notification { get; set; } = null!;
}