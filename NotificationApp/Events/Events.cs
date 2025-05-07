namespace NotificationApp.Events
{
    // Base notification event
    public class NotificationEvent
    {
        public string NotificationId { get; set; } = null!;
    }

    // Events for specific notification types
    public class EmailNotificationSent : NotificationEvent { }

    public class PushNotificationSent : NotificationEvent { }
}