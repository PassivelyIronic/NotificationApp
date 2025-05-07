using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationApp.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Recipient { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationChannel Channel { get; set; } // Zmieniono na enum
    public DateTime ScheduledTime { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Waiting;
}
