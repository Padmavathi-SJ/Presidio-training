// Application/Services/NotificationService.cs (using repository)
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task CreateNotificationAsync(int farmId, int? adminId, int? workerId, string title, string message, string type)
    {
        var notification = new Notification
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = workerId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false
        };

        await _notificationRepository.CreateAsync(notification);
    }
}