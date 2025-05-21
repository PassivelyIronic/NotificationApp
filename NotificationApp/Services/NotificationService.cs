using MassTransit;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(Notification notification);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMessageScheduler _scheduler;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IPublishEndpoint publishEndpoint,
        IMessageScheduler scheduler,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        notification.Status = NotificationStatus.Waiting;
        notification.RetryCount = 0;

        await _repository.InsertAsync(notification);

        _logger.LogInformation($"Created notification {notification.Id} for {notification.Recipient} scheduled at {notification.ScheduledTime}");

        var delay = notification.ScheduledTime - DateTime.UtcNow;

        if (delay > TimeSpan.Zero)
        {
            _logger.LogInformation($"Scheduling notification {notification.Id} to be sent at {notification.ScheduledTime} " +
                                  $"(in {delay.TotalMinutes:F1} minutes)");

            await _scheduler.ScheduleSend(
                new Uri("queue:notification-created"),
                notification.ScheduledTime,
                new ScheduleNotification { NotificationId = notification.Id! });
        }
        else
        {
            _logger.LogInformation($"Notification {notification.Id} is scheduled for immediate delivery");

            notification.Status = NotificationStatus.Scheduled;
            await _repository.UpdateAsync(notification);

            await _publishEndpoint.Publish(notification);
        }
    }
}