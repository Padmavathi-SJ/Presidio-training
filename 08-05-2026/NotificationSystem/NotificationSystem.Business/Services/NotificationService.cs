using System;
using NotificationSystem.Business.Config;
using NotificationSystem.DataAccess.Models;
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

        // send notification to a specific user
        public async Task<bool> SendToUser(int userId, NotificationType type, string subject, string message)
        {
            var user = _userService.GetUserById(userId);
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

        // send notification to all active users
        public async Task<(int sent, int failed)> SendToAllUsers(NotificationType type, string subject, string message)
        {
            var users = _userService.GetAllUsers().Where(u => u.IsActive).ToList();
            int sentCount = 0;
            int failedCount = 0;

            foreach(var user in users)
            {
                try
                {
                    bool success = await SendToUser(user.Id, type, subject, message);
                    if(success) sentCount++;
                    else
                    failedCount++;
                } catch
                {
                    failedCount++;
                }
            } return (sentCount, failedCount);
        }

        // SEND email to user
        public async Task<bool> SendEmailToUser(int userId, string subject, string message)
        {
            return await SendToUser(userId, NotificationType.Email, subject, message);
        }

        // send sms to users
        public async Task<bool> SendSmsToUser(int userId, string message)
        {
            return await SendToUser(userId, NotificationType.Sms, "", message);
        }

        // Get all notificaiton history
        public List<Notification> GetAllNotifications()
        {
            var entities = _notificationRepository.GetAll();
            return entities.Select(MapToBusinessModel).ToList();
        }

        // Get notifications for a specific user
        public List<Notification> GetUserNotifications(int userId)
        {
            var entities = _notificationRepository.GetByUserId(userId);
            return entities.Select(MapToBusinessModel).ToList();
        }

        // get sent notifications only
        public List<Notification> GetSentNotifications()
        {
            var entities = _notificationRepository.GetSentNotifications();
            return entities.Select(MapToBusinessModel).ToList();
        }

        // create Notification from stored data
      

// Private helper methods
        private async Task<bool> SendAndSaveNotification(INotification notification)
        {
            // Send the notification
            bool success = await notification.SendAsync();
            
            // Create entity for storage
            var entity = new NotificationEntity
            {
                Id = notification.Id,
                UserId = notification.UserId,
                UserName = notification.UserName,
                Type = notification.GetNotificationType(),
                Subject = notification is EmailNotification email ? email.Subject : "",
                Message = notification.Message,
                Recipient = notification.Recipient,
                IsSent = notification.IsSent,
                SentAt = notification.SentAt,
                ErrorMessage = notification.ErrorMessage ?? ""
            };

                        // Save to repository
            _notificationRepository.Add(entity);
            
            return success;
        }
private Notification MapToBusinessModel(NotificationEntity entity)
        {
            var notification = new Notification
            {
                Id = entity.Id,
                UserId = entity.UserId,
            
                Type = entity.Type == "Email" ? NotificationType.Email : NotificationType.Sms,
                Subject = entity.Subject,
                Message = entity.Message,
                Recipient = entity.Recipient,
                IsSent = entity.IsSent,
                SentAt = entity.SentAt,
                ErrorMessage = entity.ErrorMessage
            };
            return notification;
        }
    }
}


