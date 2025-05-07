using MassTransit;
using NotificationApp.Models;
using NotificationApp.Repositories;

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

            if (notification.Channel == NotificationChannel.Push)
            {
                _logger.LogInformation($"Sending push notification to {notification.Recipient} with message: {notification.Message}");
                // Implementacja wysyłki push
                notification.Status = NotificationStatus.Sent;
                await _repository.UpdateAsync(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing push notification.");
        }
    }
}