using System;
using System.Collections.Generic;
using NotifySystem.Notifications;

namespace NotifySystem.Interfaces
{
    public interface INotificationRepository
    {
        // create
        EmailNotification Create(EmailNotification notification);

        // Read
        EmailNotification? GetById(int id);
        List<EmailNotification>? GetAll();  // get all notifications

        // update
        EmailNotification? Update(int id, EmailNotification notification);

        // delete
        bool Delete(int id);
    }
}