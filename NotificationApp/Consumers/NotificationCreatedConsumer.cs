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
            _logger.LogInformation($"Processing notification {notification.Id} for {notification.Recipient} via {notification.Channel} scheduled at {notification.ScheduledTime}");

            notification.Status = NotificationStatus.Scheduled;
            await _repository.UpdateAsync(notification);

            switch (notification.Channel)
            {
                case NotificationChannel.Email:
                    var emailEndpoint = await context.GetSendEndpoint(new Uri($"{context.SourceAddress.Scheme}://{context.SourceAddress.Host}/email-notification"));
                    await emailEndpoint.Send(new EmailNotificationSent { NotificationId = notification.Id! });
                    break;

                case NotificationChannel.Push:
                    var pushEndpoint = await context.GetSendEndpoint(new Uri($"{context.SourceAddress.Scheme}://{context.SourceAddress.Host}/push-notification"));
                    await pushEndpoint.Send(new PushNotificationSent { NotificationId = notification.Id! });
                    break;

                default:
                    _logger.LogWarning($"Unknown notification channel: {notification.Channel}");
                    break;
            }
        }
    }
}