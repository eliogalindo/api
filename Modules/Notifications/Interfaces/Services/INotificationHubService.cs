using api.Modules.Notifications.DTOs;

namespace api.Modules.Notifications.Interfaces.Services;

public interface INotificationHubService
{
    Task SendNotificationSummary(int userId, NotificationSummaryDto summary);
    Task HandleUserConnected(string userId, string connectionId, string culture);
    Task HandleUserDisconnected(string connectionId);
}