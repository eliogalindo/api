using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Models;

namespace api.Modules.Notifications.Mappers;

public static class NotificationsMapper
{
    public static NotificationDto ToNotificationDto(this UserNotification userNotification, string localizedMessage)
    {
        return new NotificationDto
        {
            Id = userNotification.NotificationId,
            Message = localizedMessage,
            Type = userNotification.Notification.Type,
            IsRead = userNotification.IsRead,
            CreatedAt = userNotification.Notification.CreatedAt
        };
    }
}