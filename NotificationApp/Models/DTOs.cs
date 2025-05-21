namespace NotificationApp.Models.DTOs
{
    public class CreateNotificationRequest
    {
        public string Recipient { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationChannel Channel { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}
