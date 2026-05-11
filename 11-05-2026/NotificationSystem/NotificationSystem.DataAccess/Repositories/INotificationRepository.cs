using System;
using System.Collections.Generic;
using NotificationSystem.DataAccess.Models;

namespace NotificationSystem.DataAccess.Repositories
{
    public interface INotificationRepository
    {
        List<NotificationEntity> GetAll();
        List<NotificationEntity> GetByUserId(int userId);
        NotificationEntity? GetById(int id);
        void Add(NotificationEntity notification);
        List<NotificationEntity> GetSentNotifications();
       // void Update(NotificationEntity notification);
    }
}