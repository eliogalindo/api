using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Hubs;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Models;
using api.Modules.Users.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace api.Modules.Notifications.Services;

public class NotificationHubService(
    IHubContext<NotificationHub> hubContext,
    IUsersConnectionManagerService connectionManager,
    INotificationsQueueService notificationsQueueService,
    ILogger<NotificationHubService> logger)
    : INotificationHubService
{
    public async Task SendNotificationSummary(int userId, NotificationSummaryDto summary)
    {
        var connections = connectionManager.GetUserConnections(userId.ToString()).ToList();
        if (connections.Count == 0) return;

        await hubContext.Clients.Clients(connections).SendAsync("ReceiveNotificationSummary", summary);
    }

    public Task HandleUserConnected(string userId, string connectionId, string culture)
    {
        connectionManager.AddConnection(userId, connectionId);

        // Use the culture passed from the SignalR hub
        notificationsQueueService.Writer.TryWrite(new NotificationQueueMessage(int.Parse(userId), culture));

        logger.LogInformation("User {UserId} connected with connection ID {ConnectionId} and culture {Culture}",
            userId, connectionId, culture);

        return Task.CompletedTask;
    }

    public Task HandleUserDisconnected(string connectionId)
    {
        connectionManager.RemoveConnection(connectionId);
        logger.LogInformation("User disconnected with connection ID {ConnectionId}", connectionId);
        return Task.CompletedTask;
    }
}