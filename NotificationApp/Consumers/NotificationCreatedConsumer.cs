using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers
{
    public class NotificationCreatedConsumer : IConsumer<Notification>
    {
        private readonly ILogger<NotificationCreatedConsumer> _logger;
        private readonly INotificationRepository _repository;

        public NotificationCreatedConsumer(
            ILogger<NotificationCreatedConsumer> logger,
            INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<Notification> context)
        {
            var notification = context.Message;
            _logger.LogInformation($"Notification received for {notification.Recipient} via {notification.Channel} at {notification.ScheduledTime}. Message: {notification.Message}");

            // Update status to scheduled
            notification.Status = NotificationStatus.Scheduled;
            await _repository.UpdateAsync(notification);

            // Publish appropriate event based on the notification channel
            switch (notification.Channel)
            {
                case NotificationChannel.Email:
                    await context.Publish(new EmailNotificationSent { NotificationId = notification.Id! });
                    break;
                case NotificationChannel.Push:
                    await context.Publish(new PushNotificationSent { NotificationId = notification.Id! });
                    break;
                default:
                    _logger.LogWarning($"Unknown notification channel: {notification.Channel}");
                    break;
            }
        }
    }
}