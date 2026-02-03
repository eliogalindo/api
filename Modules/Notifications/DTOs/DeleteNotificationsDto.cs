using System.ComponentModel.DataAnnotations;

namespace api.Modules.Notifications.DTOs;

public class DeleteNotificationsDto
{
    [Required] [MinLength(1)] public int[] NotificationIds { get; set; } = [];
}