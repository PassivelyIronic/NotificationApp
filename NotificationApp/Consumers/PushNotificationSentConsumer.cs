using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers
{
    public sealed class PushNotificationSentConsumer : IConsumer<PushNotificationSent>
    {
        private readonly ILogger<PushNotificationSentConsumer> _logger;
        private readonly INotificationRepository _repository;

        public PushNotificationSentConsumer(
            ILogger<PushNotificationSentConsumer> logger,
            INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<PushNotificationSent> context)
        {
            try
            {
                var eventMessage = context.Message;

                // Get the notification from database
                var notification = await _repository.GetByIdAsync(eventMessage.NotificationId);

                if (notification == null)
                {
                    _logger.LogError($"Notification not found: {eventMessage.NotificationId}");
                    return;
                }

                _logger.LogInformation($"Sending push notification to {notification.Recipient} with message: {notification.Message}");

                // Push notification implementation would go here
                // ...

                // Update notification status
                notification.Status = NotificationStatus.Sent;
                await _repository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing push notification.");
            }
        }
    }
}