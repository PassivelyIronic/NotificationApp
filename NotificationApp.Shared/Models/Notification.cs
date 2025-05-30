﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NotificationApp.Shared.Models;

namespace NotificationApp.Shared.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Recipient { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationChannel Channel { get; set; }
    public DateTime ScheduledTime { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Waiting;

    public int RetryCount { get; set; } = 0;
    public DateTime? LastRetryTime { get; set; }
}