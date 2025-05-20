using MassTransit;
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

    public NotificationService(
        INotificationRepository repository,
        IPublishEndpoint publishEndpoint,
        IMessageScheduler scheduler)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _scheduler = scheduler;
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        notification.Status = NotificationStatus.Waiting;
        notification.RetryCount = 0;
        await _repository.InsertAsync(notification);

        // Calculate delay time (if in the future)
        var delay = notification.ScheduledTime - DateTime.UtcNow;

        // Only schedule if it's in the future
        if (delay > TimeSpan.Zero)
        {
            await _scheduler.SchedulePublish(notification.ScheduledTime, notification);
        }
        else
        {
            // If it's for now or past, publish immediately
            await _publishEndpoint.Publish(notification);
        }
    }
}