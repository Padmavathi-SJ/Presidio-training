using System;
using System.Reflection.Metadata;

namespace NotificationSystem.DataAccess.Entities
{
    public class UserEntity
    {
        public int Id {get; set;}
        public string Name {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public string PhoneNum {get; set;} = string.Empty;
        public bool IsActive {get; set;} 
        public bool ReceiveEmailNotification {get; set;}
        public bool ReceiveSmsNotification {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
    }
}