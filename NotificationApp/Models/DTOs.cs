namespace NotificationApp.Models.DTOs
{
    public class CreateNotificationRequest
    {
        public string Recipient { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationChannel Channel { get; set; } // Zmieniono na enum
        public DateTime ScheduledTime { get; set; }
    }
}
