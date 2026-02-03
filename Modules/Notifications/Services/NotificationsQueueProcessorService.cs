using System.Globalization;
using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Interfaces.Services;

namespace api.Modules.Notifications.Services;

public class NotificationsQueueProcessorService(
    INotificationsQueueService notificationsQueueService,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationsQueueProcessorService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification queue processor started.");

        await foreach (var message in notificationsQueueService.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();

            // Set the culture for this thread
            var culture = new CultureInfo(message.Culture);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var notificationsService = scope.ServiceProvider.GetRequiredService<INotificationsService>();
            var hubService = scope.ServiceProvider.GetRequiredService<INotificationHubService>();

            var result = await notificationsService.GetRecentNotifications(message.UserId, 5);

            if (!result.IsSuccess) continue;

            var (notifications, count) = result.Data;
            await hubService.SendNotificationSummary(message.UserId, new NotificationSummaryDto
            {
                UnreadCount = count,
                RecentNotifications = notifications
            });
        }

        logger.LogInformation("Notification queue processor stopped.");
    }
}