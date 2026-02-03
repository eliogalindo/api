using api.Modules.Notifications.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace api.Modules.Notifications.Hubs;

public class NotificationHub(INotificationHubService notificationHubService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            await base.OnConnectedAsync();
            return;
        }

        // Extract culture from SignalR connection context
        var culture = GetCultureFromConnection();

        // Pass culture to the hub service
        await notificationHubService.HandleUserConnected(userId, Context.ConnectionId, culture);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await notificationHubService.HandleUserDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private string GetCultureFromConnection()
    {
        // Try to get culture from query parameters first (for WebSocket connections)
        if (Context.GetHttpContext()?.Request.Query.TryGetValue("locale", out var localeQuery) == true &&
            !string.IsNullOrEmpty(localeQuery.ToString()))
            return localeQuery.ToString();

        // Try to get from headers
        var httpContext = Context.GetHttpContext();
        if (httpContext?.Request.Headers.TryGetValue("X-User-Locale", out var localeHeader) == true &&
            !string.IsNullOrEmpty(localeHeader.ToString()))
            return localeHeader.ToString();

        // Try Accept-Language as fallback
        if (httpContext?.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage) != true ||
            string.IsNullOrEmpty(acceptLanguage.ToString())) return "en-US";
        var firstLanguage = acceptLanguage.ToString().Split(',').FirstOrDefault();
        return !string.IsNullOrEmpty(firstLanguage)
            ? firstLanguage.Trim()
            :
            // Default culture
            "en-US";
    }
}