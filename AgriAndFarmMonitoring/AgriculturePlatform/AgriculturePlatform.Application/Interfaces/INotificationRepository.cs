// Application/Interfaces/INotificationRepository.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUnreadByUserAsync(int farmId, int? adminId, int? workerId);
    Task<IEnumerable<Notification>> GetRecentByUserAsync(int farmId, int? adminId, int? workerId, int limit = 50);
    Task<Notification?> GetUnreadAggregateByTypeAsync(int farmId, int? adminId, int? workerId, string type);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int farmId, int? adminId, int? workerId);
    Task UpdateAsync(Notification notification);
}