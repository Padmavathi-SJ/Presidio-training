using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using NotificationSystem.Business.Models;
using NotificationSystem.Business.Services;
using NotificationSystem.Business.Config;
using NotificationSystem.DataAccess.Repositories;

namespace NotificationSystem.Presentation
{
    class Program
    {
        private static UserService _userService = null!;
        private static NotificationService _notificationService = null!;
        private static SmtpConfig _smtpConfig = null!;

        static async Task Main(string[] args)
        {
            Console.Title = "Notification System";

            Env.Load();

            // Configure SMTP from environment variables
            _smtpConfig = new SmtpConfig
            {
                Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com",
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"),
                SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? string.Empty,
                SenderPassword = Environment.GetEnvironmentVariable("SMTP_SENDER_PASSWORD") ?? string.Empty,
                EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true")
            };

            // Initialize repositories and services
            var userRepository = new UserRepository();
            var notificationRepository = new NotificationRepository();

            _userService = new UserService(userRepository);
            _notificationService = new NotificationService(notificationRepository, _userService, _smtpConfig);

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("1. Add new User");
                Console.WriteLine("2. View All Users");
                Console.WriteLine("3. Send to specific user");
                Console.WriteLine("4. Send to all Users");
                Console.WriteLine("5. View All Notifications");
                Console.WriteLine("6. View User Notifications");
                Console.WriteLine("7. Exit");
                Console.Write("\nChoose an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await AddUser();
                        break;

                    case "2":
                        ViewAllUsers();
                        break;

                    case "3":
                        await SendNotificationToUser();
                        break;

                    case "4":
                        await SendToAllUsers();
                        break;

                    case "5":
                        ViewAllNotifications();
                        break;

                    case "6":
                        ViewUserNotifications();
                        break;

                    case "7":
                        running = false;
                        Console.WriteLine("\nThank you for using Notification system!");
                        break;

                    default:
                        Console.WriteLine("\nInvalid option! Please try again.");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        static async Task AddUser()
        {
            Console.Clear();
            Console.WriteLine("Add new User");

            try
            {
                Console.Write("\nName: ");
                string? name = Console.ReadLine();

                Console.Write("Email: ");
                string? email = Console.ReadLine();

                Console.Write("Phone number (10 digits): ");
                string? phone = Console.ReadLine();

                var user = _userService.CreateUser(name ?? "", email ?? "", phone ?? "");
                Console.WriteLine($"\nUser created successfully with ID: {user.Id}");

                // Ask for notification preferences
                Console.Write("\nEnable email notification? (Y/N): ");
                bool receiveEmail = Console.ReadLine()?.ToUpper() == "Y";

                Console.Write("Enable SMS Notification? (Y/N): ");
                bool receiveSms = Console.ReadLine()?.ToUpper() == "Y";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

        static void ViewAllUsers()
        {
            var users = _userService.GetAllUsers();

            Console.Clear();
            Console.WriteLine("ALL USERS");

            if (users.Count == 0)
            {
                Console.WriteLine("\nNo users found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Email",-30} {"Phone",-15}");
            Console.WriteLine(new string('-', 75));

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id,-5} {user.Name,-25} {user.Email,-30} {user.PhoneNumber,-15}");
            }
            Console.WriteLine($"\nTotal Users: {users.Count}");
        }

        static async Task SendNotificationToUser()
        {
            Console.Clear();
            var users = _userService.GetAllUsers();

            if (users.Count == 0)
            {
                Console.WriteLine("No users available. Add a user first.");
                return;
            }

            Console.WriteLine("SEND TO SPECIFIC USER");
            Console.WriteLine("\nAvailable Users: ");

            foreach (var user in users)
            {
                Console.WriteLine($"   {user.Id}. {user.Name} - {user.Email}");
            }

            Console.Write("\nEnter user ID: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Invalid ID!");
                return;
            }

            var selectedUser = users.FirstOrDefault(u => u.Id == userId);
            if (selectedUser == null)
            {
                Console.WriteLine("User not found!");
                return;
            }

            Console.WriteLine("\nNotification Type:");
            Console.WriteLine(" 1. Email");
            Console.WriteLine(" 2. SMS");
            Console.Write("Choose: ");
            string? typeChoice = Console.ReadLine();

            try
            {
                bool success = false;

                if (typeChoice == "1") // email
                {
                    Console.Write("Subject: ");
                    string? subject = Console.ReadLine();

                    Console.Write("Message: ");
                    string? message = Console.ReadLine();

                    success = await _notificationService.SendEmailToUser(userId, subject ?? "", message ?? "");
                }
                else if (typeChoice == "2") // sms
                {
                    Console.Write("Message: ");
                    string? message = Console.ReadLine();

                    success = await _notificationService.SendSmsToUser(userId, message ?? "");
                }
                else
                {
                    Console.WriteLine("Invalid notification type!");
                    return;
                }

                if (success)
                {
                    Console.WriteLine($"\nNotification sent successfully to {selectedUser.Name}!");
                }
                else
                {
                    Console.WriteLine($"\nFailed to send notification to {selectedUser.Name}!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

        static async Task SendToAllUsers()
        {
            Console.Clear();

            Console.WriteLine("SEND TO ALL USERS");
            var users = _userService.GetAllUsers().Where(u => u.IsActive).ToList();

            if (users.Count == 0)
            {
                Console.WriteLine("No active users available.");
                return;
            }

            Console.WriteLine($"\nWill send to {users.Count} active user(s).\n");

            Console.WriteLine("Notification Type:");
            Console.WriteLine("   1. Email");
            Console.WriteLine("   2. SMS");
            Console.Write("   Choose: ");
            string? typeChoice = Console.ReadLine();

            try
            {
                bool useEmail = typeChoice == "1";
                string subject = "";

                if (useEmail)
                {
                    Console.Write("Subject: ");
                    subject = Console.ReadLine() ?? "";
                }

                Console.Write("Message: ");
                string? message = Console.ReadLine();

                int sent = 0;
                int failed = 0;

                foreach (var user in users)
                {
                    bool success;
                    if (useEmail)
                        success = await _notificationService.SendEmailToUser(user.Id, subject, message ?? "");
                    else
                        success = await _notificationService.SendSmsToUser(user.Id, message ?? "");

                    if (success) sent++;
                    else failed++;

                    Console.WriteLine($"  {user.Name}.... {(success ? "Yes" : "No")}");
                }

                Console.WriteLine($"\nSummary: {sent} sent, {failed} failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

        static void ViewAllNotifications()
        {
            var notifications = _notificationService.GetAllNotifications();

            Console.Clear();
            Console.WriteLine("ALL NOTIFICATIONS");

            if (notifications.Count == 0)
            {
                Console.WriteLine("\nNo notifications found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Type",-8} {"To",-25} {"Message",-35} {"Status",-15}");
            Console.WriteLine(new string('-', 90));

            foreach (var n in notifications)
            {
                string content = n.Type == NotificationType.Email ? n.Subject : (n.Message.Length > 30 ? n.Message.Substring(0, 27) + "..." : n.Message);
                string status = n.IsSent ? $"Sent at {n.SentAt:HH:mm}" : n.ErrorMessage ?? "Failed";

                Console.WriteLine($"{n.Id,-5} {n.Type,-8} {n.Recipient,-25} {content,-35} {status,-15}");
            }
        }

        static void ViewUserNotifications()
        {
            var users = _userService.GetAllUsers();

            if (users.Count == 0)
            {
                Console.WriteLine("No users available.");
                return;
            }

            Console.Clear();
            Console.WriteLine("USER NOTIFICATIONS");
            Console.WriteLine("\nSelect User:");

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id}. {user.Name}");
            }

            Console.Write("\nEnter user ID: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Invalid ID!");
                return;
            }

            var notifications = _notificationService.GetUserNotifications(userId);

            if (notifications.Count == 0)
            {
                Console.WriteLine($"\nNo notifications found for this user.");
                return;
            }

            Console.WriteLine($"\nNotifications for user ID {userId}:");

            foreach (var n in notifications)
            {
                string details = n.Type == NotificationType.Email
                    ? $"Subject: {n.Subject}"
                    : $"Message: {(n.Message.Length > 40 ? n.Message.Substring(0, 37) + "..." : n.Message)}";

                string status = n.IsSent ? $"Sent at {n.SentAt:yyyy-MM-dd HH:mm}" : $"Failed: {n.ErrorMessage}";
                Console.WriteLine($"[{n.Id}] {n.Type}: {details}");
                Console.WriteLine($"   {status}");
            }
            Console.WriteLine($"\nTotal: {notifications.Count} notifications");
        }
    }
}