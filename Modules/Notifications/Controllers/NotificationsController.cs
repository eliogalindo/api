using System.Security.Claims;
using api.Core.Controllers;
using api.Modules.Localization.Interfaces.Services;
using api.Modules.Notifications.DTOs;
using api.Modules.Notifications.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Modules.Notifications.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NotificationsController(
    ILocalizationService localizationService,
    INotificationsService notificationsService)
    : LocalizedControllerBase(localizationService)
{
    [Authorize(Policy = "RequireReadNotifications")]
    [HttpGet]
    public async Task<IActionResult> FindAll([FromQuery] NotificationsSearchParamsDto notificationsSearchParamsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await notificationsService.FindUserNotifications(int.Parse(userId), notificationsSearchParamsDto);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireWriteNotifications")]
    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result =
            await notificationsService.MarkUserNotificationAsRead(int.Parse(userId), int.Parse(notificationId));
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireWriteNotifications")]
    [HttpPatch]
    public async Task<IActionResult> MarkAllNotificationAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await notificationsService.MarkAllUserNotificationsAsRead(int.Parse(userId));
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteNotifications")]
    [HttpDelete]
    public async Task<IActionResult> DeleteMany([FromBody] DeleteNotificationsDto deleteNotificationsDto)
    {
        var result = await notificationsService.DeleteManyNotifications(deleteNotificationsDto.NotificationIds);
        return HandleServiceResult(result);
    }

    [Authorize(Policy = "RequireDeleteNotifications")]
    [HttpDelete("{Id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await notificationsService.DeleteNotification(id);
        return HandleServiceResult(result);
    }
}