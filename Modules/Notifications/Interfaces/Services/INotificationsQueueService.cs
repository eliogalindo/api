using System.Threading.Channels;
using api.Modules.Notifications.Models;

namespace api.Modules.Notifications.Interfaces.Services;

public interface INotificationsQueueService
{
    ChannelWriter<NotificationQueueMessage> Writer { get; }
    ChannelReader<NotificationQueueMessage> Reader { get; }
}