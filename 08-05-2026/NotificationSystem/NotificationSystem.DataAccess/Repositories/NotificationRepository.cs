using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using NotificationSystem.DataAccess.Models;

namespace NotificationSystem.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _filePath = "notifications.json";
        private List<NotificationEntity> _notifications;

        public NotificationRepository()
        {
            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _notifications = JsonSerializer.Deserialize<List<NotificationEntity>>(json) ?? new List<NotificationEntity>();
            }
            else
            {
                _notifications = new List<NotificationEntity>();
                SaveData();
            }
        }

        private void SaveData()
        {
            var json = JsonSerializer.Serialize(_notifications);
            File.WriteAllText(_filePath, json);
        }

        public List<NotificationEntity> GetAll()
        {
            return _notifications.OrderByDescending(n => n.SentAt).ToList();
        }

        public NotificationEntity? GetById(int id)
        {
            return _notifications.FirstOrDefault(n => n.Id == id);
        }

        public List<NotificationEntity> GetByUserId(int userId)
        {
            return _notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .ToList();
        }

        public void Add(NotificationEntity notification)
        {
            notification.Id = _notifications.Count > 0 ? _notifications.Max(n => n.Id) + 1 : 1;
            _notifications.Add(notification);
            SaveData();
        }

        public void Update(NotificationEntity notification)
        {
            var existing = GetById(notification.Id);
            if (existing != null)
            {
                existing.IsSent = notification.IsSent;
                existing.SentAt = notification.SentAt;
                existing.ErrorMessage = notification.ErrorMessage;
                SaveData();
            }
        }

        public List<NotificationEntity> GetSentNotifications()
        {
            return _notifications
                .Where(n => n.IsSent)
                .OrderByDescending(n => n.SentAt)
                .ToList();
        }
    }
}