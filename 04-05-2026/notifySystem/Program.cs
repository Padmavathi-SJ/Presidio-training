using System;
using System.Collections.Generic;
using NotifySystem.Models;
using NotifySystem.Services;
using NotifySystem.Notifications;
using NotifySystem.Config;

namespace NotifySystem
{
    class Program{
        static async Task Main(string[] args){
            // configure SMTP with your credentials
            var smtpConfig = new SmtpConfig{
                Host = "smtp.gmail.com",
                Port = 587,
                SenderEmail = "padmavathisj2005@gmail.com",
                SenderPassword = "izqvhduaqfhtlvcl",
                EnableSsl = true
            };

            // create users
            var users = new List<User>{
                new User("Alia", "padmasj54@gmail.com", "8300770817"),
                new User("Balia", "padmavathi.cs22@bitsathy.ac.in", "8300770817"),
            };

            // create notifications
            var emailNotification = new EmailNotification(
                "your presidio account has been created successfully",
                "welcome to Genspark training",
                smtpConfig
            );

            var smsNotification = new SMSNotification("your otp is 123456 to create presidio account, welcome to Genspark training");

            // create service
            var service = new NotificationService();

            // send notifications
            Console.WriteLine("sending email notifications....");
            await service.SendToAll(emailNotification, users);

            Console.WriteLine("\nsending SMS notifications...");
            await service.Send(smsNotification, users[0]);

            // sending multiple notifications to one user
            Console.WriteLine("\nmultiple notifications to same user...");
            var notifications = new List<INotification>{
                new EmailNotification("the training is going very well!!", smtpConfig),
                new SMSNotification("Need to learn more about C#"),
                new EmailNotification("looking forward to the next module!!!", smtpConfig)
            };

            await service.SendMultiple(notifications, users[1]);

            Console.WriteLine("\nAll notifications processed successfully!");
        }
    }
}