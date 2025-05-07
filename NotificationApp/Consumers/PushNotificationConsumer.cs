using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers;

public class PushNotificationConsumer : IConsumer<Notification>
{
    private readonly ILogger<PushNotificationConsumer> _logger;
    private readonly INotificationRepository _repository;

    public PushNotificationConsumer(ILogger<PushNotificationConsumer> logger, INotificationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<Notification> context)
    {
        try
        {
            var notification = context.Message;

            // Double-check this is actually a push notification
            if (notification.Channel != NotificationChannel.Push)
            {
                _logger.LogWarning($"Non-push notification received in push queue: ID={notification.Id}, Channel={notification.Channel}");
                return;
            }

            _logger.LogInformation($"Processing push notification: ID={notification.Id}, Recipient={notification.Recipient}");

            // Simulate sending push notification
            _logger.LogInformation($"Sending push notification to {notification.Recipient} with message: {notification.Message}");

            // Update notification status
            notification.Status = NotificationStatus.Sent;
            await _repository.UpdateAsync(notification);

            _logger.LogInformation($"Push notification processed successfully: ID={notification.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing push notification");

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