using System;
using System.Reflection.Metadata;
using NotificationSystem.Business.Models;

namespace NotificationSystem.Business.Interfaces
{
    public interface INotification
    {
        //common properties for all notificaton types
        int Id {get; set; }
        int UserId {get; set; }
        string UserName {get; set;}

        string Message {get; set;}
        string Recipient {get; set;}
        bool IsSent {get; set;}
        DateTime SentAt {get; set;}
        string? ErrorMessage {get; set;}

       // common methods
       Task<bool> SendAsync();
       string GetNotificationType();

    }
}