using System;
using NotifySystem.Models;

namespace NotifySystem.Notifications{
    // another implementation of same interface INotification, this provides a specific implementation for sending SMS notification
    public class SMSNotification: INotification{
        public string Message { get; set; }
        public DateTime SentDate { get; set; }

        // SMS specific property
        public int CharacterCount { get; private set; } // only class can modify

        // constructor with validation
        public SMSNotification(string message){
            Message = message;
            SentDate = DateTime.Now;
            CharacterCount = message.Length;
            
            //SMS Validation (160 character limit)
            if(CharacterCount > 160){
                Console.WriteLine("Warning: SMS exceeds 160 characters. will be split.");
            }
        }

        // interface method implementation: defines how to send SMS notification
        public async Task Send(User recipient){
            Console.WriteLine($"\n SENDING SMS...");
            Console.WriteLine($"To: {recipient.PhoneNumber}");
            Console.WriteLine($"Message: {Message}");
            Console.WriteLine($"Characters: {CharacterCount}/160");
            Console.WriteLine($"Sent at: {SentDate}");
            Console.WriteLine($"SMS sent successfully to {recipient.Name}");
            await Task.CompletedTask;
        }

        public string GetDeliveryMethod(){
            return "SMS";
        }

        // additional method - SMS specific
        public bool IsTooLong(){
            return CharacterCount > 160;
        }

        public override string ToString(){
            return $"[SMS] {Message.Substring(0, Math.Min(20, Message.Length))}...";
        }
    }
}