using MassTransit;
using NotificationApp.Models;

namespace NotificationApp.Consumers
{
    public interface INotificationConsumer : IConsumer<Notification>
    {
        Task ConsumeNotificationAsync(Notification notification);
    }
}
