using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers;

public class EmailNotificationConsumer : IConsumer<Notification>
{
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private readonly INotificationRepository _repository;

    public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger, INotificationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<Notification> context)
    {
        try
        {
            var notification = context.Message;

            // Double-check this is actually an email notification
            if (notification.Channel != NotificationChannel.Email)
            {
                _logger.LogWarning($"Non-email notification received in email queue: ID={notification.Id}, Channel={notification.Channel}");
                return;
            }

            _logger.LogInformation($"Processing email notification: ID={notification.Id}, Recipient={notification.Recipient}");

            // Simulate sending email
            _logger.LogInformation($"Sending email to {notification.Recipient} with message: {notification.Message}");

            // Update notification status
            notification.Status = NotificationStatus.Sent;
            await _repository.UpdateAsync(notification);

            _logger.LogInformation($"Email notification processed successfully: ID={notification.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing email notification");

            // Update status to failed if possible
            if (context.Message?.Id != null)
            {
                try
                {
                    var notification = context.Message;
                    notification.Status = NotificationStatus.Failed;
                    await _repository.UpdateAsync(notification);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update notification status to failed");
                }
            }
        }
    }
}