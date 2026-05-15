using System;
using System.Collections.Generic;
using NotificationSystem.DataAccess.Entities;

namespace NotificationSystem.DataAccess.Repositories
{
    public interface INotificationRepository
    {
        Task<List<NotificationEntity>> GetAllAsync();
        Task<List<NotificationEntity>> GetByUserIdAsync(int userId);
        Task<NotificationEntity?> GetByIdAsync(int id);
        Task<NotificationEntity> AddAsync(NotificationEntity notification);
        Task<List<NotificationEntity>> GetSentNotificationsAsync();
        Task<List<NotificationEntity>> GetUnSentNotificationsAsync();
        Task MarkAsSentAsync (int id, DateTime sentAt);
        Task MarkAsFailedAsync (int id, string errorMessage);
       // void Update(NotificationEntity notification);
    }
}