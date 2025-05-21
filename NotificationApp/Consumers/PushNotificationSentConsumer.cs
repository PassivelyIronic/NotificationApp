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

                var notification = await _repository.GetByIdAsync(eventMessage.NotificationId);

                if (notification == null)
                {
                    _logger.LogError($"Notification not found: {eventMessage.NotificationId}");
                    return;
                }

                await _repository.IncrementRetryCountAsync(notification.Id!);
                notification.RetryCount++;

                _logger.LogInformation($"Attempt {notification.RetryCount} of sending push notification to {notification.Recipient} with message: {notification.Message}");

                bool isSuccessful = _random.Next(100) < 50;

                if (!isSuccessful)
                {
                    _logger.LogWarning($"Simulated failure for notification {notification.Id} (attempt {notification.RetryCount})");

                    if (notification.RetryCount >= 3)
                    {
                        _logger.LogError($"Notification {notification.Id} has failed after {notification.RetryCount} attempts");
                        notification.Status = NotificationStatus.Failed;
                        await _repository.UpdateAsync(notification);
                        return;
                    }

                    throw new Exception("Simulated push delivery failure (50% chance)");
                }

                notification.Status = NotificationStatus.Sent;
                await _repository.UpdateAsync(notification);
                _logger.LogInformation($"Successfully sent push notification to {notification.Recipient} after {notification.RetryCount} attempts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing push notification.");
                throw;
            }
        }
    }
}