using NotificationApp.Shared.Models;

namespace NotificationApp.Shared.Repositories;

public interface INotificationRepository
{
    Task InsertAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(string id);
    Task IncrementRetryCountAsync(string id);
    Task<List<Notification>> GetScheduledNotificationsAsync();
}