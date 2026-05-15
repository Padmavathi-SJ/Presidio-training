using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationSystem.Business.Config;
using NotificationSystem.DataAccess.Entities;
using NotificationSystem.DataAccess.Repositories;
using NotificationSystem.Business.Models;
using NotificationSystem.Business.Notifications;
using NotificationSystem.Business.Interfaces;

namespace NotificationSystem.Business.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserService _userService;
        private readonly SmtpConfig _smtpConfig;

        public NotificationService(INotificationRepository notificationRepository, UserService userService, SmtpConfig smtpconfig)
        {
            _notificationRepository = notificationRepository;
            _userService = userService;
            _smtpConfig = smtpconfig;
        }

        public async Task<bool> SendToUser(int userId, NotificationType type, string subject, string message)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if(user == null)
                throw new ArgumentException($"User with ID {userId} not found");

            INotification notification;
            if(type == NotificationType.Email)
            {
                if(!user.ReceiveEmailNotifications)
                    throw new InvalidOperationException($"User {user.Name} does not want to receive emails");

                notification = new EmailNotification(_smtpConfig)
                {
                    UserId = userId,
                    UserName = user.Name,
                    Subject = subject,
                    Message = message,
                    Recipient = user.Email,
                    IsSent = false
                };
            }
            else
            {
                if(!user.ReceiveSmsNotifications)
                    throw new InvalidOperationException($"User {user.Name} does not want to receive SMS");
                
                notification = new SmsNotification
                {
                    UserId = userId,
                    UserName = user.Name,
                    Message = message,
                    Recipient = user.PhoneNumber,
                    IsSent = false
                };
            }

            return await SendAndSaveNotification(notification);
        }

        public async Task<(int sent, int failed)> SendToAllUsers(NotificationType type, string subject, string message)
        {
            var users = (await _userService.GetAllUsersAsync()).Where(u => u.IsActive).ToList();
            int sentCount = 0;
            int failedCount = 0;

            foreach(var user in users)
            {
                try
                {
                    bool success = await SendToUser(user.Id, type, subject, message);
                    if(success) sentCount++;
                    else failedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }
            return (sentCount, failedCount);
        }

        public async Task<bool> SendEmailToUser(int userId, string subject, string message)
        {
            return await SendToUser(userId, NotificationType.Email, subject, message);
        }

        public async Task<bool> SendSmsToUser(int userId, string message)
        {
            return await SendToUser(userId, NotificationType.Sms, "", message);
        }

        public List<Notification> GetAllNotifications()
        {
            return GetAllNotificationsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            var entities = await _notificationRepository.GetAllAsync();
            return entities.Select(MapToBusinessModel).ToList();
        }

        public List<Notification> GetUserNotifications(int userId)
        {
            return GetUserNotificationsAsync(userId).GetAwaiter().GetResult();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            var entities = await _notificationRepository.GetByUserIdAsync(userId);
            return entities.Select(MapToBusinessModel).ToList();
        }

        // Helper method to convert Business NotificationType to string for database
        private string TypeToString(NotificationType type)
        {
            return type == NotificationType.Email ? "Email" : "Sms";
        }

        // Helper method to convert string from database to Business NotificationType
        private NotificationType StringToType(string type)
        {
            return type == "Email" ? NotificationType.Email : NotificationType.Sms;
        }

        private async Task<bool> SendAndSaveNotification(INotification notification)
        {
            bool success = await notification.SendAsync();
            
            var entity = new NotificationEntity
            {
                UserId = notification.UserId,
                UserName = notification.UserName,
                Type = notification.GetNotificationType(), // This returns "Email" or "Sms" as string
                Subject = notification is EmailNotification email ? email.Subject : "",
                Message = notification.Message,
                Recipient = notification.Recipient,
                IsSent = notification.IsSent,
                SentAt = notification.SentAt,
                ErrorMessage = notification.ErrorMessage ?? "",
                CreatedAt = DateTime.UtcNow 
            };

            await _notificationRepository.AddAsync(entity);
            
            return success;
        }

        private Notification MapToBusinessModel(NotificationEntity entity)
        {
            var notification = new Notification
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Type = StringToType(entity.Type), // Convert string to enum
                Subject = entity.Subject,
                Message = entity.Message,
                Recipient = entity.Recipient,
                IsSent = entity.IsSent,
                SentAt = entity.SentAt ?? DateTime.UtcNow,
                ErrorMessage = entity.ErrorMessage,
                CreatedAt = entity.CreatedAt
            };
            return notification;
        }
    }
}