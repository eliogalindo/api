using api.Core.Data;
using api.Core.DTOs;
using api.Core.Repositories;
using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Interfaces.Repositories;
using api.Modules.Notifications.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.Notifications.Repositories;

public class NotificationsRepository(AppDbContext dbContext)
    : Repository<Notification>(dbContext), INotificationsRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var userNotification = await _dbContext.UserNotifications
            .FirstOrDefaultAsync(un =>
                un.UserId == userId &&
                un.NotificationId == notificationId);

        if (userNotification == null) return false;

        userNotification.IsRead = true;
        userNotification.ReadDate = DateTime.UtcNow;

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var unreadNotifications = await _dbContext.UserNotifications
            .Where(un => un.UserId == userId && !un.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0) return false;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
        }

        return true;
    }

    public async Task<bool> AssignNotificationToUsersAsync(List<int> userIds, int notificationId)
    {
        var userNotifications = userIds.Select(userId => new UserNotification
        {
            UserId = userId,
            NotificationId = notificationId,
            IsRead = false
        });

        await _dbContext.UserNotifications.AddRangeAsync(userNotifications);
        return true;
    }

    public async Task<(List<UserNotification> UserNotifications, int Count)> GetRecentUserNotificationsAsync(int userId,
        int limit = 5)
    {
        var query = _dbContext.UserNotifications.AsQueryable();

        var count = query.Count(un => un.UserId == userId && !un.IsRead);

        var notifications = await query
            .Where(un => un.UserId == userId && !un.IsRead)
            .OrderByDescending(un => un.Notification.CreatedAt)
            .Include(un => un.Notification)
            .Take(limit)
            .ToListAsync();

        return (notifications, count);
    }


    public async Task<(List<UserNotification>UserNotifications, int Count)> FindUserNotificationsAsync(int userId,
        SearchParamsDto searchParamsDto)
    {
        var query = _dbContext.UserNotifications.AsQueryable();
        if (searchParamsDto is NotificationsSearchParamsDto notificationSearchParamsDto)
        {
            if (userId > 0)
                query = query.Where(un => un.UserId == userId);

            if (!notificationSearchParamsDto.IncludeRead)
                query = query.Where(un => !un.IsRead);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParamsDto.OrderBy))

            query = searchParamsDto.OrderBy.ToLower() switch
            {
                "type" => searchParamsDto.Desc
                    ? query.OrderByDescending(un => un.Notification.Type)
                    : query.OrderBy(un => un.Notification.Type),
                "isRead" => searchParamsDto.Desc
                    ? query.OrderByDescending(un => un.IsRead)
                    : query.OrderBy(un => un.IsRead),
                _ => searchParamsDto.Desc
                    ? query.OrderByDescending(un => un.Notification.CreatedAt)
                    : query.OrderBy(un => un.Notification.CreatedAt)
            };
        else
            // Default sorting if no OrderBy provided
            query = searchParamsDto.Desc
                ? query.OrderByDescending(un => un.NotificationId)
                : query.OrderBy(un => un.NotificationId);

        // Get the total count for pagination
        var count = await query.CountAsync();

        var notifications = await query
            .Include(un => un.Notification)
            .Skip((searchParamsDto.PageNumber - 1) * searchParamsDto.PageSize)
            .Take(searchParamsDto.PageSize)
            .ToListAsync();

        return (notifications, count);
    }
}