using api.Core.DTOs;
using api.Core.Interfaces.Repositories;
using api.Modules.Notifications.Models;

namespace api.Modules.Notifications.Interfaces.Repositories;

public interface INotificationsRepository : IRepository<Notification>
{
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<bool> AssignNotificationToUsersAsync(List<int> userIds, int notificationId);

    Task<(List<UserNotification>UserNotifications, int Count)> FindUserNotificationsAsync(int userId,
        SearchParamsDto searchParamsDto);

    Task<(List<UserNotification>UserNotifications, int Count)> GetRecentUserNotificationsAsync(int userId, int limit);
}