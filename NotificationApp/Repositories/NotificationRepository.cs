using MongoDB.Driver;
using NotificationApp.Models;

namespace NotificationApp.Repositories
{
    public interface INotificationRepository
    {
        Task InsertAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task<Notification?> GetByIdAsync(string id);
        Task IncrementRetryCountAsync(string id);
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
                .Set(n => n.Status, notification.Status)
                .Set(n => n.ScheduledTime, notification.ScheduledTime)
                .Set(n => n.RetryCount, notification.RetryCount)
                .Set(n => n.LastRetryTime, notification.LastRetryTime);

            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<Notification?> GetByIdAsync(string id)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task IncrementRetryCountAsync(string id)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notification>.Update
                .Inc(n => n.RetryCount, 1)
                .Set(n => n.LastRetryTime, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}