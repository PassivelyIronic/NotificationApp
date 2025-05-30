﻿using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Consumers
{
    public class ScheduledNotificationConsumer : IConsumer<ScheduleNotification>
    {
        private readonly ILogger<ScheduledNotificationConsumer> _logger;
        private readonly INotificationRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ScheduledNotificationConsumer(
            ILogger<ScheduledNotificationConsumer> logger,
            INotificationRepository repository,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<ScheduleNotification> context)
        {
            _logger.LogInformation($"Received scheduled notification request for notification {context.Message.NotificationId}");

            var notification = await _repository.GetByIdAsync(context.Message.NotificationId);

            if (notification == null)
            {
                _logger.LogError($"Notification {context.Message.NotificationId} not found for scheduling");
                return;
            }

            notification.Status = NotificationStatus.Scheduled;
            await _repository.UpdateAsync(notification);

            _logger.LogInformation($"Publishing notification {notification.Id} for {notification.Recipient} to be processed");
            await _publishEndpoint.Publish(notification);
        }
    }
}