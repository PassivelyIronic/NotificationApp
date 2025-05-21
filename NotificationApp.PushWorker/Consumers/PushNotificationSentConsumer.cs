// NotificationApp.PushWorker/Consumers/PushNotificationSentConsumer.cs
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Shared.Events;
using NotificationApp.Shared.Models;
using NotificationApp.Shared.Repositories;

namespace NotificationApp.PushWorker.Consumers;

public sealed class PushNotificationSentConsumer : IConsumer<PushNotificationSent>
{
    private readonly ILogger<PushNotificationSentConsumer> _logger;
    private readonly INotificationRepository _repository;
    private readonly Random _random = new Random();

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

            // Increment retry count
            await _repository.IncrementRetryCountAsync(notification.Id!);
            notification.RetryCount++;

            _logger.LogInformation($"Attempt {notification.RetryCount} of sending push notification to {notification.Recipient} with message: {notification.Message}");

            // Simulate 50% success rate
            bool isSuccessful = _random.Next(100) < 50;

            if (!isSuccessful)
            {
                _logger.LogWarning($"Simulated failure for notification {notification.Id} (attempt {notification.RetryCount})");

                // If we've reached max retries, mark as failed
                if (notification.RetryCount >= 3)
                {
                    _logger.LogError($"Notification {notification.Id} has failed after {notification.RetryCount} attempts");
                    notification.Status = NotificationStatus.Failed;
                    await _repository.UpdateAsync(notification);
                    return;
                }

                // Otherwise throw exception to trigger retry
                throw new Exception("Simulated push delivery failure (50% chance)");
            }

            // Update notification status on success
            notification.Status = NotificationStatus.Sent;
            await _repository.UpdateAsync(notification);
            _logger.LogInformation($"Successfully sent push notification to {notification.Recipient} after {notification.RetryCount} attempts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing push notification.");
            throw; // Rethrow to trigger retry
        }
    }
}