using System;
using System.Collections.Generic;
using NotifySystem.Models;
using NotifySystem.Notifications;


namespace NotifySystem.Services
{
    public class NotificationService
    {
        public async Task Send(INotification notification, User recipient)
        {
            Console.WriteLine($"\nSending {notification.GetDeliveryMethod()} to {recipient.Name}");
            await notification.Send(recipient);
        }

        public async Task SendToAll(INotification notification, List<User> users)
        {
            foreach(var user in users)
            {
                await Send(notification, user);
            }
        }

        public async Task SendMultiple(List<INotification> notifications, User recipient)
        {
            foreach(var notif in notifications)
            {
                await Send(notif, recipient);
            }
        }
    }
}