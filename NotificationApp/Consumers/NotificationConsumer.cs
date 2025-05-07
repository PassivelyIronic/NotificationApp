using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Models;

namespace NotificationApp.Consumers;

public class NotificationConsumer : IConsumer<Notification>
{
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public NotificationConsumer(ILogger<NotificationConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<Notification> context)
    {
        var notification = context.Message;
        _logger.LogInformation($"Notification received for {notification.Recipient} via {notification.Channel} at {notification.ScheduledTime}. Message: {notification.Message}");

        // Process only notifications with Waiting status
        if (notification.Status == NotificationStatus.Waiting)
        {
            // Instead of publishing a new message with the same data, use the SendEndpoint to route to specific queues
            string queueName = notification.Channel switch
            {
                NotificationChannel.Email => "email-notification-queue",
                NotificationChannel.Push => "push-notification-queue",
                _ => throw new ArgumentOutOfRangeException()
            };

            // Get the send endpoint for the appropriate queue
            var endpoint = await context.GetSendEndpoint(new Uri($"{context.SourceAddress.Scheme}://{context.SourceAddress.Host}/{queueName}"));

            // Send the notification to the appropriate queue
            await endpoint.Send(notification);

            _logger.LogInformation($"Notification {notification.Id} routed to {queueName}");
        }
        else
        {
            _logger.LogInformation($"Skipping notification {notification.Id} with status {notification.Status}");
        }
    }
}