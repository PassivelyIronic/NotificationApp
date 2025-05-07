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

            if (notification.Channel == NotificationChannel.Email)
            {
                _logger.LogInformation($"Sending email to {notification.Recipient} with message: {notification.Message}");
                // Implementacja wysyłki emaila
                notification.Status = NotificationStatus.Sent;
                await _repository.UpdateAsync(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing email notification.");
        }
    }
}