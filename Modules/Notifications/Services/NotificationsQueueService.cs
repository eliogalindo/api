using System.Threading.Channels;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Models;

namespace api.Modules.Notifications.Services;

public class NotificationsQueueService : INotificationsQueueService
{
    private readonly Channel<NotificationQueueMessage> _channel = Channel.CreateUnbounded<NotificationQueueMessage>();

    public ChannelWriter<NotificationQueueMessage> Writer => _channel.Writer;
    public ChannelReader<NotificationQueueMessage> Reader => _channel.Reader;
}