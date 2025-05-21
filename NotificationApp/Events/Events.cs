namespace NotificationApp.Events
{
    public class NotificationEvent
    {
        public string NotificationId { get; set; } = null!;
    }

    public class EmailNotificationSent : NotificationEvent { }

    public class PushNotificationSent : NotificationEvent { }

    public class ScheduleNotification : NotificationEvent { }
}