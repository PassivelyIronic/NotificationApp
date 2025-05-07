using Microsoft.AspNetCore.Mvc;
using NotificationApp.Models;
using NotificationApp.Models.DTOs;
using NotificationApp.Services;

namespace NotificationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateNotificationRequest request)
        {
            // Tworzymy nową notyfikację, przekazując dane z żądania
            var notification = new Notification
            {
                Recipient = request.Recipient,
                Message = request.Message,
                Channel = request.Channel, // Przekazujemy enum Channel
                ScheduledTime = request.ScheduledTime,
                Status = NotificationStatus.Waiting
            };

            // Przekazujemy powiadomienie do usługi
            await _service.CreateNotificationAsync(notification);

            // Zwracamy powiadomienie jako odpowiedź
            return Ok(notification);
        }
    }
}
