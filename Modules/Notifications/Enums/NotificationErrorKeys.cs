using api.Core.Attributes;

namespace api.Modules.Notifications.Enums;

public enum NotificationErrorKeys
{
    [FallbackMessage("The selected notification doesn't exist")]
    NotificationNotFound,

    [FallbackMessage("The selected notifications don't exist")]
    NotificationsNotFound
}