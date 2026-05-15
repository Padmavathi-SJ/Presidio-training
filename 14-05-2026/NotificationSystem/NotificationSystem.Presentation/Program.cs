using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationSystem.Business.Models;
using NotificationSystem.Business.Services;
using NotificationSystem.Business.Config;
using NotificationSystem.DataAccess.Repositories;
using NotificationSystem.DataAccess.Config;
using NotificationSystem.DataAccess.Context;

namespace NotificationSystem.Presentation
{
    class Program
    {
        private static IServiceProvider _serviceProvider = null!;
        private static UserService _userService = null!;
        private static NotificationService _notificationService = null!;
        private static SmtpConfig _smtpConfig = null!;

        static async Task Main(string[] args)
        {
            Console.Title = "Notification System";

            // Load .env from solution root (same as Word Guessing Game)
            Env.Load();

            // Database config (same pattern as Word Guessing Game)
            var dbConfig = new DatabaseConfig
            {
                Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
                DatabaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "notification_system_ef",
                UserName = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
            };

            // SMTP config
            _smtpConfig = new SmtpConfig
            {
                Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com",
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"),
                SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? string.Empty,
                SenderPassword = Environment.GetEnvironmentVariable("SMTP_SENDER_PASSWORD") ?? string.Empty,
                EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true")
            };

            // Test database connection (same as Word Guessing Game)
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(dbConfig.GetConnectionString());
                
                using var testContext = new ApplicationDbContext(optionsBuilder.Options);
                await testContext.Database.CanConnectAsync();
                Console.WriteLine("Database connected successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // Setup dependency injection
            _serviceProvider = ConfigureServices(dbConfig);

            // Apply migrations
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("Migrations applied successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration failed: {ex.Message}");
                }
            }

            // Get services
            _userService = _serviceProvider.GetRequiredService<UserService>();
            _notificationService = _serviceProvider.GetRequiredService<NotificationService>();

            // Main menu loop
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("=== NOTIFICATION SYSTEM ===");
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
                        Console.WriteLine("\nThank you for using Notification System!");
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

        private static IServiceProvider ConfigureServices(DatabaseConfig databaseConfig)
        {
            var services = new ServiceCollection();

            services.AddSingleton(databaseConfig);
            services.AddSingleton(_smtpConfig);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(databaseConfig.GetConnectionString()));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<UserService>();
            services.AddScoped<NotificationService>();

            return services.BuildServiceProvider();
        }

        static async Task AddUser()
        {
            Console.Clear();
            Console.WriteLine("=== ADD NEW USER ===");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                if (ex.InnerException != null)
        {
            Console.WriteLine($"\nDetailed Error: {ex.InnerException.Message}");
        }
            }
            
        }

        static void ViewAllUsers()
        {
            var users = _userService.GetAllUsers();

            Console.Clear();
            Console.WriteLine("=== ALL USERS ===");

            if (users.Count == 0)
            {
                Console.WriteLine("\nNo users found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Email",-30} {"Phone",-15} {"Active",-8} {"Email Notif",-12} {"SMS Notif",-10}");
            Console.WriteLine(new string('-', 110));

            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id,-5} {user.Name,-25} {user.Email,-30} {user.PhoneNumber,-15} {(user.IsActive ? "Yes" : "No"),-8} {(user.ReceiveEmailNotifications ? "Yes" : "No"),-12} {(user.ReceiveSmsNotifications ? "Yes" : "No"),-10}");
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

            Console.WriteLine("=== SEND TO SPECIFIC USER ===");
            Console.WriteLine("\nAvailable Users:");

            foreach (var user in users)
            {
                Console.WriteLine($"   {user.Id}. {user.Name} - {user.Email} {(user.IsActive ? "" : "(Inactive)")}");
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

            if (!selectedUser.IsActive)
            {
                Console.WriteLine("This user is inactive. Cannot send notifications.");
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

                if (typeChoice == "1")
                {
                    if (!selectedUser.ReceiveEmailNotifications)
                    {
                        Console.WriteLine("This user has disabled email notifications.");
                        return;
                    }

                    Console.Write("Subject: ");
                    string? subject = Console.ReadLine();

                    Console.Write("Message: ");
                    string? message = Console.ReadLine();

                    success = await _notificationService.SendEmailToUser(userId, subject ?? "", message ?? "");
                }
                else if (typeChoice == "2")
                {
                    if (!selectedUser.ReceiveSmsNotifications)
                    {
                        Console.WriteLine("This user has disabled SMS notifications.");
                        return;
                    }

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
            Console.WriteLine("=== SEND TO ALL USERS ===");

            var users = _userService.GetAllUsers();
            var activeUsers = users.Where(u => u.IsActive).ToList();

            if (activeUsers.Count == 0)
            {
                Console.WriteLine("No active users available.");
                return;
            }

            Console.WriteLine($"\nWill send to {activeUsers.Count} active user(s).\n");

            Console.WriteLine("Notification Type:");
            Console.WriteLine("   1. Email");
            Console.WriteLine("   2. SMS");
            Console.Write("   Choose: ");
            string? typeChoice = Console.ReadLine();

            try
            {
                NotificationType type = typeChoice == "1" ? NotificationType.Email : NotificationType.Sms;
                string subject = "";

                if (type == NotificationType.Email)
                {
                    Console.Write("Subject: ");
                    subject = Console.ReadLine() ?? "";
                }

                Console.Write("Message: ");
                string? message = Console.ReadLine();

                var result = await _notificationService.SendToAllUsers(type, subject, message ?? "");

                Console.WriteLine("\n--- Summary ---");
                Console.WriteLine($"Sent: {result.sent}");
                Console.WriteLine($"Failed: {result.failed}");
                Console.WriteLine($"Total: {activeUsers.Count}");
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
            Console.WriteLine("=== ALL NOTIFICATIONS ===");

            if (notifications.Count == 0)
            {
                Console.WriteLine("\nNo notifications found.");
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Type",-8} {"To",-25} {"Message/Subject",-35} {"Status",-20}");
            Console.WriteLine(new string('-', 100));

            foreach (var n in notifications)
            {
                string content = n.Type == NotificationType.Email
                    ? (n.Subject?.Length > 32 ? n.Subject.Substring(0, 29) + "..." : n.Subject ?? "No Subject")
                    : (n.Message.Length > 32 ? n.Message.Substring(0, 29) + "..." : n.Message);

                string status = n.IsSent
                    ? $"Sent at {n.SentAt:HH:mm}"
                    : (n.ErrorMessage != null ? n.ErrorMessage : "Failed");

                Console.WriteLine($"{n.Id,-5} {n.Type,-8} {n.Recipient,-25} {content,-35} {status,-20}");
            }
            Console.WriteLine($"\nTotal Notifications: {notifications.Count}");
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
            Console.WriteLine("=== USER NOTIFICATIONS ===");
            Console.WriteLine("\nSelect User:");

            foreach (var user in users)
            {
                Console.WriteLine($"  {user.Id}. {user.Name} - {user.Email}");
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

            var notifications = _notificationService.GetUserNotifications(userId);

            Console.Clear();
            Console.WriteLine($"=== NOTIFICATIONS FOR {selectedUser.Name.ToUpper()} ===");

            if (notifications.Count == 0)
            {
                Console.WriteLine($"\nNo notifications found for this user.");
                return;
            }

            Console.WriteLine();

            foreach (var n in notifications)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"ID: {n.Id} | Type: {n.Type} | Status: {(n.IsSent ? "Sent" : "Failed")}");

                if (n.Type == NotificationType.Email && !string.IsNullOrEmpty(n.Subject))
                    Console.WriteLine($"Subject: {n.Subject}");

                Console.WriteLine($"Message: {n.Message}");
                Console.WriteLine($"Recipient: {n.Recipient}");

                if (n.IsSent)
                    Console.WriteLine($"Sent At: {n.SentAt:yyyy-MM-dd HH:mm:ss}");
                else if (!string.IsNullOrEmpty(n.ErrorMessage))
                    Console.WriteLine($"Error: {n.ErrorMessage}");

                Console.WriteLine($"Created: {n.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine();
            }

            Console.WriteLine($"Total: {notifications.Count} notifications");
        }
    }
}