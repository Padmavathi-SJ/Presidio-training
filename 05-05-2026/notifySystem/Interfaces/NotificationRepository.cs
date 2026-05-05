using System;
using System.Collections.Generic;
using System.Linq;
using NotifySystem.Interfaces;
using NotifySystem.Notifications;

namespace NotifySystem.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        public Dictionary<int, EmailNotification> notificationsDb;
        
        private int next_id;

        public NotificationRepository(){
            notificationsDb = new Dictionary<int, EmailNotification>();
            next_id = 1;
        }

        // create
        public EmailNotification Create(EmailNotification notification){
            if(notification == null){
                throw new ArgumentNullException(nameof(notification));
            }
            var id = next_id++;
            notification.Id = id;
            notificationsDb.Add(id, notification);
            return notification;
        }

        // Read
        public EmailNotification? GetById(int id){
            return notificationsDb.ContainsKey(id) ? notificationsDb[id] : null;
        }

        // get all notifications
        public List<EmailNotification>? GetAll(){
            if(notificationsDb.Count == 0){
                return null;
            }
            return notificationsDb.Values.ToList();
        }

        // update
        public EmailNotification? Update(int id, EmailNotification updatedNotification){
           if(!notificationsDb.ContainsKey(id)){
            return null;
           }
           updatedNotification.Id = id;
           notificationsDb[id] = updatedNotification;
           return updatedNotification;
        }

        // delete
        public bool Delete(int id){
            if(!notificationsDb.ContainsKey(id)){
                return false;
            }
            notificationsDb.Remove(id);
            return true;
        }

    }
}
