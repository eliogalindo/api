using System.Text.Json;
using api.Core.DTOs;
using api.Core.Enums;
using api.Core.Interfaces.Services;
using api.Core.Models;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Enums;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Mappers;
using api.Modules.Notifications.Models;
using api.Modules.Users.Enums;

namespace api.Modules.Notifications.Services;

public class NotificationsService(
    IUnitOfWork unitOfWork,
    INotificationsQueueService notificationsQueueService,
    ILocalizationService localizationService,
    ILogger<NotificationsService> logger) : INotificationsService
{
    public async Task<ServiceResult<bool>> MarkUserNotificationAsRead(int userId, int notificationId)
    {
        try
        {
            var result = await unitOfWork.NotificationsRepository.MarkAsReadAsync(userId, notificationId);
            await unitOfWork.SaveChangesAsync();

            var culture = localizationService.GetCurrentCulture();
            await notificationsQueueService.Writer.WriteAsync(new NotificationQueueMessage(userId, culture));

            return ServiceResult<bool>.Success(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while marking the notification as read");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> MarkAllUserNotificationsAsRead(int userId)
    {
        try
        {
            var result = await unitOfWork.NotificationsRepository.MarkAllAsReadAsync(userId);
            await unitOfWork.SaveChangesAsync();

            var culture = localizationService.GetCurrentCulture();
            await notificationsQueueService.Writer.WriteAsync(new NotificationQueueMessage(userId, culture));
            return ServiceResult<bool>.Success(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while marking all notifications as read");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<(List<NotificationDto>, int)>> GetRecentNotifications(int userId, int limit = 5)
    {
        try
        {
            List<NotificationDto> notificationDtos = [];
            var (notifications, count) =
                await unitOfWork.NotificationsRepository.GetRecentUserNotificationsAsync(userId, limit);
            notificationDtos.AddRange(from notification in notifications
                let args = notification.Notification.LocalizationArgs != null
                    ? JsonSerializer.Deserialize<string[]>(notification.Notification.LocalizationArgs)
                    : Array.Empty<object>()
                let localizedMessage =
                    localizationService.GetLocalizedString(notification.Notification.LocalizationKey, args)
                select MapToDto(notification, localizedMessage));

            return ServiceResult<(List<NotificationDto>, int)>.Success((notificationDtos, count));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while finding user notifications");
            return ServiceResult<(List<NotificationDto>, int)>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<PagedResultDto<NotificationDto>>> FindUserNotifications(int userId,
        SearchParamsDto searchParamsDto)
    {
        try
        {
            List<NotificationDto> notificationDtos = [];
            var (userNotifications, totalCount) =
                await unitOfWork.NotificationsRepository.FindUserNotificationsAsync(userId, searchParamsDto);

            notificationDtos.AddRange(from userNotification in userNotifications
                let args = userNotification.Notification.LocalizationArgs != null
                    ? JsonSerializer.Deserialize<string[]>(userNotification.Notification.LocalizationArgs)
                    : Array.Empty<object>()
                let localizedMessage =
                    localizationService.GetLocalizedString(userNotification.Notification.LocalizationKey, args)
                select MapToDto(userNotification, localizedMessage));

            var pagedResultDto = new PagedResultDto<NotificationDto>
            {
                Items = notificationDtos,
                Count = totalCount,
                PageNumber = searchParamsDto.PageNumber,
                PageSize = searchParamsDto.PageSize
            };

            return ServiceResult<PagedResultDto<NotificationDto>>.Success(pagedResultDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while finding user notifications");
            return ServiceResult<PagedResultDto<NotificationDto>>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> DeleteNotification(int id)
    {
        try
        {
            await unitOfWork.NotificationsRepository.DeleteAsync(id);
            await unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting notification");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> DeleteManyNotifications(int[] ids)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            await unitOfWork.NotificationsRepository.DeleteManyAsync(ids);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(e, "Error deleting notifications");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    public async Task<ServiceResult<bool>> CreateNotification(string localizationKey, NotificationType notificationType,
        List<int> userIds, params object[] args)
    {
        try
        {
            var localizationArgs = args is { Length: > 0 } ? JsonSerializer.Serialize(args) : null;

            var notification = new Notification
            {
                LocalizationKey = localizationKey,
                LocalizationArgs = localizationArgs,
                Type = notificationType,
                CreatedAt = DateTime.UtcNow
            };

            // Save notification
            await unitOfWork.NotificationsRepository.CreateAsync(notification);
            await unitOfWork.SaveChangesAsync();

            // Assign notification to users and enqueue their IDs
            var result = await AssignNotificationToUsers(userIds, notification.Id);

            if (!result.IsSuccess) return result;

            var culture = localizationService.GetCurrentCulture();
            foreach (var userId in userIds)
                await notificationsQueueService.Writer.WriteAsync(new NotificationQueueMessage(userId, culture));

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating notification");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private async Task<ServiceResult<bool>> AssignNotificationToUsers(List<int> userIds, int notificationId)
    {
        try
        {
            var result =
                await unitOfWork.NotificationsRepository.AssignNotificationToUsersAsync(userIds, notificationId);
            return result
                ? ServiceResult<bool>.Success(result)
                : ServiceResult<bool>.Failure(UserErrorKeys.UserNotFound, ServiceErrorType.NotFound);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while assigning notification to users");
            return ServiceResult<bool>.Failure(GenericErrorKeys.InternalError);
        }
    }

    private static NotificationDto MapToDto(UserNotification userNotification, string localizedMessage)
    {
        return userNotification.ToNotificationDto(localizedMessage);
    }
}