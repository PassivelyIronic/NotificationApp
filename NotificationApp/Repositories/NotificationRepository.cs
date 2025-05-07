using MongoDB.Driver;
using NotificationApp.Models;

namespace NotificationApp.Repositories;

public interface INotificationRepository
{
    Task InsertAsync(Notification notification);
    Task UpdateAsync(Notification notification); // Dodana metoda
}

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Notification>("Notifications");
    }

    public async Task InsertAsync(Notification notification)
    {
        await _collection.InsertOneAsync(notification);
    }

    public async Task UpdateAsync(Notification notification)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notification.Id);
        var update = Builders<Notification>.Update
            .Set(n => n.Status, notification.Status) // Przykład: aktualizacja statusu, możesz dodać inne pola
            .Set(n => n.ScheduledTime, notification.ScheduledTime); // Aktualizacja daty

        await _collection.UpdateOneAsync(filter, update);
    }
}

