using api.Core.DTOs;
using api.Core.Models;
using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Enums;

namespace api.Modules.Notifications.Interfaces.Services;

public interface INotificationsService
{
    Task<ServiceResult<bool>> CreateNotification(string localizationKey, NotificationType notificationType,
        List<int> userIds, params object[] args);

    Task<ServiceResult<PagedResultDto<NotificationDto>>> FindUserNotifications(int userId,
        SearchParamsDto searchParamsDto);

    Task<ServiceResult<bool>> MarkUserNotificationAsRead(int userId, int notificationId);
    Task<ServiceResult<bool>> MarkAllUserNotificationsAsRead(int userId);
    Task<ServiceResult<(List<NotificationDto>, int)>> GetRecentNotifications(int userId, int limit);
    Task<ServiceResult<bool>> DeleteNotification(int id);
    Task<ServiceResult<bool>> DeleteManyNotifications(int[] ids);
}