// Application/Interfaces/INotificationRepository.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUnreadByUserAsync(int farmId, int? adminId, int? workerId);
    Task MarkAsReadAsync(int notificationId);
}