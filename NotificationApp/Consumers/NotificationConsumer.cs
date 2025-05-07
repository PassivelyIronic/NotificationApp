using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Models;

namespace NotificationApp.Consumers;

public class NotificationConsumer : IConsumer<Notification>
{
    private readonly ILogger<NotificationConsumer> _logger;

    public NotificationConsumer(ILogger<NotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Notification> context)
    {
        var notification = context.Message;
        _logger.LogInformation($"Notification received for {notification.Recipient} via {notification.Channel} at {notification.ScheduledTime}. Message: {notification.Message}");

        // Sprawdź, czy status jest już przypisany do odpowiedniego kanału (np. email lub push)
        if (notification.Status == NotificationStatus.Waiting)
        {
            // Publikowanie do odpowiednich kolejek na podstawie statusu
            await context.Publish<Notification>(new
            {
                notification.Recipient,
                notification.Message,
                notification.Channel,
                notification.ScheduledTime,
                Status = notification.Status.ToString().ToLower() // Przekazujemy status jako routing key
            });
        }
    }
}

