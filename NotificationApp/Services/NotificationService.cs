using MassTransit;
using NotificationApp.Models;
using NotificationApp.Repositories;

namespace NotificationApp.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(Notification notification);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public NotificationService(INotificationRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        notification.Status = NotificationStatus.Waiting;
        await _repository.InsertAsync(notification);
        await _publishEndpoint.Publish(notification);
    }
}
