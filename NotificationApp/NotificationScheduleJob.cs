using MassTransit;
using MongoDB.Driver;
using NotificationApp.Models;
using NotificationApp.Repositories;
using Quartz;

namespace NotificationApp;

public class NotificationScheduleJob : IJob
{
    private readonly INotificationRepository _repository;
    private readonly IBus _bus;
    private readonly ILogger<NotificationScheduleJob> _logger;

    public NotificationScheduleJob(
        INotificationRepository repository,
        IBus bus,
        ILogger<NotificationScheduleJob> logger)
    {
        _repository = repository;
        _bus = bus;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Checking for scheduled notifications to process");

        var collection = ((NotificationRepository)_repository).GetCollection();
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.Status, NotificationStatus.Waiting),
            Builders<Notification>.Filter.Lte(n => n.ScheduledTime, DateTime.UtcNow)
        );

        var notifications = await collection.Find(filter).ToListAsync();

        _logger.LogInformation($"Found {notifications.Count} notifications ready to be processed");

        foreach (var notification in notifications)
        {
            _logger.LogInformation($"Processing scheduled notification {notification.Id} for {notification.Recipient}");

            notification.Status = NotificationStatus.Scheduled;
            await _repository.UpdateAsync(notification);

            await _bus.Publish(notification);
        }
    }
}
