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
            var notification = new Notification
            {
                Recipient = request.Recipient,
                Message = request.Message,
                Channel = request.Channel,
                ScheduledTime = request.ScheduledTime,
                Status = NotificationStatus.Waiting
            };

            await _service.CreateNotificationAsync(notification);

            return Ok(notification);
        }
    }
}
