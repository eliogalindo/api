using api.Modules.Notifications.Hubs;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Services;
using api.Modules.Users.Interfaces.Services;
using api.Modules.Users.Services;

namespace api.Core.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        // Add SignalR
        services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 102400; // 100KB
            options.EnableDetailedErrors = true; // For development
        });

        // Add a connection manager
        services.AddSingleton<IUsersConnectionManagerService, UsersConnectionManagerService>();

        // Notification service
        services.AddScoped<INotificationHubService, NotificationHubService>();
        services.AddScoped<INotificationsService, NotificationsService>();

        return services;
    }

    public static IApplicationBuilder UseSignalREndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<NotificationHub>("/signalR/notificationhub");
            // Add more hub mappings as needed
        });
        return app;
    }
}