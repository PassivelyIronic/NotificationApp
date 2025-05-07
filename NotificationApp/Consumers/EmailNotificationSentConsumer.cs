using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers
{
    public sealed class EmailNotificationSentConsumer : IConsumer<EmailNotificationSent>
    {
        private readonly ILogger<EmailNotificationSentConsumer> _logger;
        private readonly INotificationRepository _repository;

        public EmailNotificationSentConsumer(
            ILogger<EmailNotificationSentConsumer> logger,
            INotificationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<EmailNotificationSent> context)
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

                _logger.LogInformation($"Sending email to {notification.Recipient} with message: {notification.Message}");

                // Email sending implementation would go here
                // ...

                // Update notification status
                notification.Status = NotificationStatus.Sent;
                await _repository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing email notification.");
            }
        }
    }
}