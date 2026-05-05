using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetEnv;
using NotifySystem.Models;
using NotifySystem.Services;
using NotifySystem.Notifications;
using NotifySystem.Config;
using NotifySystem.Interfaces;
using NotifySystem.Repositories;

namespace NotifySystem
{
    class Program{
        static async Task Main(string[] args){
            // load environment variables from .env file
            Env.Load();

            // configure SMTP from .env credentials
            var smtpConfig = new SmtpConfig{
                Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com",
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"),
                SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? string.Empty,
                SenderPassword = Environment.GetEnvironmentVariable("SMTP_SENDER_PASSWORD") ?? string.Empty,
                EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true")
            };

            // create users
            var users = new List<User>{
                new User("Alia", "padmasj54@gmail.com", "8300770817"),
                new User("Balia", "padmavathi.cs22@bitsathy.ac.in", "8300770817"),
            };

            NotificationRepository notificationRepo = new NotificationRepository();

            bool running = true;

            while(running){
                
                Console.WriteLine("enter an option to continue to the process of creating and sending the notifications, and get all available notification, and updating and deleting as well");
                Console.WriteLine("1. create email notification");
                Console.WriteLine("2. get all available notifications");
                Console.WriteLine("3. get notification by id");
                Console.WriteLine("4. update notification");
                Console.WriteLine("5. delete notification");
                Console.WriteLine("6. send notification");
                Console.WriteLine("7. exit");

                Console.Write("Your choice: ");
                int choice = int.Parse(Console.ReadLine() ?? "0");
                
                switch(choice){
                    case 1:
                        await CreateNotification(notificationRepo, smtpConfig);
                        break;
                    case 2:
                        await GetAllNotifications(notificationRepo);
                        break;
                    case 3:
                        await GetNotificationById(notificationRepo);
                        break;
                    case 4:
                        await UpdateNotification(notificationRepo, smtpConfig);
                        break;
                    case 5:
                        await DeleteNotification(notificationRepo);
                        break;
                    case 6:
                        await SendNotification(notificationRepo, users);
                        break;
                    case 7:
                        running = false;
                        Console.WriteLine("exiting the application, thank you.");
                        break;
                    default:
                        Console.WriteLine("invalid choice, please try again.");
                        break;
                }
            }
        }

        static async Task CreateNotification(NotificationRepository notificationRepo, SmtpConfig smtpConfig){
            
            Console.WriteLine("enter the subject of the email: ");
            var subject = Console.ReadLine();
            Console.WriteLine("enter the message of the email: ");
            var message = Console.ReadLine();
            
            if(string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message)){
                Console.WriteLine("subject and message cannot be empty, notification creation failed.");
                return;
            }
            
            var notification = new EmailNotification(message, subject, smtpConfig);
            notificationRepo.Create(notification);
            Console.WriteLine("notification created successfully!");
        }
               
        static async Task GetAllNotifications(NotificationRepository notificationRepo){
            
            var msgs = notificationRepo.GetAll();
            Console.WriteLine($"total notifications: {msgs?.Count ?? 0}");
            
            if(msgs == null || msgs.Count == 0){
                Console.WriteLine("no notifications found.");
                return;
            }
            
            Console.WriteLine("All available notifications: ");
            foreach(var msg in msgs){
                Console.WriteLine($"ID:{msg.Id}\nSubject: {msg.Subject}\nMessage: {msg.Message}");
            }
        }

        static async Task GetNotificationById(NotificationRepository notificationRepo){
         
            Console.WriteLine("enter the id of the notification to review: ");
            
            if(!int.TryParse(Console.ReadLine(), out int id)){
                Console.WriteLine("invalid id format.");
                return;
            }
            
            var notification = notificationRepo.GetById(id);
            if(notification != null){
                Console.WriteLine($"ID: {id}, Subject: {notification.Subject}, Message: {notification.Message}");
            } else{
                Console.WriteLine("invalid id, no notification found");
            }
        }

        static async Task UpdateNotification(NotificationRepository notificationRepo, SmtpConfig smtpConfig){  
          
            
            var existingNotifications = notificationRepo.GetAll();
            if(existingNotifications == null || existingNotifications.Count == 0){
                Console.WriteLine("no notifications available to update.");
                return;
            }
            
            // Show existing notifications
            Console.WriteLine("Existing notifications:");
            foreach(var notif in existingNotifications){
                Console.WriteLine($"ID: {notif.Id}, Subject: {notif.Subject}");
            }
            
            Console.WriteLine("enter the id of the notification to update: ");
            if(!int.TryParse(Console.ReadLine(), out int idToUpdate)){
                Console.WriteLine("invalid id format.");
                return;
            }
            
            var existing = notificationRepo.GetById(idToUpdate);
            if(existing == null){
                Console.WriteLine("invalid id, no notification found to update");
                return;
            }
            
            Console.WriteLine("enter the new subject of this email: ");
            var newSubject = Console.ReadLine();
            Console.WriteLine("enter the new message of this email: ");
            var newMessage = Console.ReadLine();
            
            if(string.IsNullOrEmpty(newSubject) || string.IsNullOrEmpty(newMessage)){
                Console.WriteLine("subject and message cannot be empty!");
                return;
            }
            
            var updatedNotification = new EmailNotification(newMessage, newSubject, smtpConfig);
            var result = notificationRepo.Update(idToUpdate, updatedNotification);
            
            if(result != null){
                Console.WriteLine("notification updated successfully!");
            } else{
                Console.WriteLine("update failed!");
            }
        }

        static async Task DeleteNotification(NotificationRepository notificationRepo){
           
            
            var existingNotifications = notificationRepo.GetAll();
            if(existingNotifications == null || existingNotifications.Count == 0){
                Console.WriteLine("no notifications available to delete.");
                return;
            }
            
            // Show existing notifications
            Console.WriteLine("Existing notifications:");
            foreach(var notif in existingNotifications){
                Console.WriteLine($"ID: {notif.Id}, Subject: {notif.Subject}");
            }
            
            Console.WriteLine("enter the id of the notification to delete: ");
            if(!int.TryParse(Console.ReadLine(), out int idToDelete)){
                Console.WriteLine("invalid id format.");
                return;
            }
            
            bool deleted = notificationRepo.Delete(idToDelete);
            if(deleted){
                Console.WriteLine("notification deleted successfully!");
            } else{
                Console.WriteLine("invalid id, no notification found to delete");
            }
        }

        static async Task SendNotification(NotificationRepository notificationRepo, List<User> users){
           
            var existingNotifications = notificationRepo.GetAll();
            if(existingNotifications == null || existingNotifications.Count == 0){
                Console.WriteLine("no notifications available to send.");
                return;
            }
            
            // Show existing notifications
            Console.WriteLine("Available notifications:");
            foreach(var notif in existingNotifications){
                Console.WriteLine($"ID: {notif.Id}, Subject: {notif.Subject}");
            }
            
            Console.WriteLine("enter the id of the notification to send to the users: ");
            if(!int.TryParse(Console.ReadLine(), out int idToSend)){
                Console.WriteLine("invalid id format.");
                return;
            }
            
            var notificationToSend = notificationRepo.GetById(idToSend);
            if(notificationToSend == null){
                Console.WriteLine("invalid id, no notification found to send");
                return;
            }
            
            var service = new NotificationService();
            await service.SendToAll(notificationToSend, users);
            Console.WriteLine("notification sent successfully to all users!");
        }
    }
}