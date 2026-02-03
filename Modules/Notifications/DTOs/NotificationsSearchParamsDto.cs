using api.Core.DTOs;
using api.Modules.Notifications.Enums;

namespace api.Modules.Notifications.DTOs;

public class NotificationsSearchParamsDto : SearchParamsDto
{
    public NotificationType NotificationType { get; set; }
    public bool IncludeRead { get; set; } = false;
}