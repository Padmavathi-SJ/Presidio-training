using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Npgsql;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.DataAccess.Entities;
using NotificationSystem.DataAccess.Context;

namespace NotificationSystem.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationEntity>> GetAllAsync()
        {
            return await _context.Notifications
                  .OrderBy(n => n.Id)
                  .ToListAsync();
        }

        public async Task<NotificationEntity?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<NotificationEntity>> GetByUserIdAsync(int UserId)
        {
            return await _context.Notifications
                    .Where(n => n.UserId == UserId)
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();
        }

        public async Task<NotificationEntity> AddAsync(NotificationEntity notification)
        {
            Console.WriteLine($"[DEBUG] Adding notification for user: {notification.UserId}, Type: {notification.Type}");
    
            _context.Notifications.Add(notification);
            var result = await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] SaveChangesAsync returned: {result} rows affected");
    Console.WriteLine($"[DEBUG] New notification ID: {notification.Id}");
            return notification;
        }

        public async Task<List<NotificationEntity>> GetSentNotificationsAsync()
        {
            return await _context.Notifications
            .Where(n => n.IsSent == true)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
        }

        public async Task<List<NotificationEntity>> GetUnSentNotificationsAsync()
        {
            return await _context.Notifications
            .Where(n => n.IsSent == false)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
        }

        public async Task  MarkAsSentAsync(int id, DateTime sentAt)
        {
            var notification = await GetByIdAsync(id);
            if(notification != null)
            {
                notification.IsSent = true;
                notification.SentAt = sentAt;
                await _context.SaveChangesAsync();
            }
        }

        public async Task  MarkAsFailedAsync(int id, string errorMessage)
        {
            var notification = await GetByIdAsync(id);

            if(notification != null)
            {
            notification.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
        }




/*
        public List<NotificationEntity> GetAll()
        {
            var notifications = new List<NotificationEntity>();

       using var conn = _dbConnection.GetConnection();
        string query = "select id, user_id, user_name, type, subject, message, recipient, is_sent, sent_at, error_message, created_at from notifications order by id";
        var cmd = new NpgsqlCommand(query, conn);

        var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                notifications.Add(new NotificationEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    Type = reader.GetString(3),
                    Subject = reader.GetString(4),
                    Message = reader.GetString(5),
                    Recipient = reader.GetString(6),
                    IsSent = reader.GetBoolean(7),
                    SentAt = reader.GetDateTime(8),
                    ErrorMessage = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
                });
            }
            return notifications;

        }

        public NotificationEntity? GetById(int id)
        {
      using var conn = _dbConnection.GetConnection();
      string query = "SELECT id, user_id, user_name, type, subject, message, recipient, is_sent, sent_at, error_message, created_at FROM notifications WHERE id = @id";
        var cmd = new NpgsqlCommand(query, conn);

        var reader = cmd.ExecuteReader();
        cmd.Parameters.AddWithValue("@id", id);
            while (reader.Read())
            {
                return new NotificationEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    Type = reader.GetString(3),
                    Subject = reader.GetString(4),
                    Message = reader.GetString(5),
                    Recipient = reader.GetString(6),
                    IsSent = reader.GetBoolean(7),
                    SentAt = reader.GetDateTime(8),
                    ErrorMessage = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
                };
            }
            return null;

        }

        public List<NotificationEntity> GetByUserId(int userId)
        {
            var notifications = new List<NotificationEntity>();
       using var conn = _dbConnection.GetConnection();
       string query = "SELECT id, user_id, user_name, type, subject, message, recipient, is_sent, sent_at, error_message, created_at FROM notifications WHERE user_id = @userId ORDER BY sent_at DESC";
        var cmd = new NpgsqlCommand(query, conn);

         
         cmd.Parameters.AddWithValue("@userId", userId);
         var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
               notifications.Add(new NotificationEntity
                {
                     Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    Type = reader.GetString(3),
                    Subject = reader.GetString(4),
                    Message = reader.GetString(5),
                    Recipient = reader.GetString(6),
                    IsSent = reader.GetBoolean(7),
                    SentAt = reader.GetDateTime(8),
                    ErrorMessage = reader.GetString(9)
                });
            }
            return notifications;
        }

        public void Add(NotificationEntity notification)
        {
      using var conn = _dbConnection.GetConnection();
       string query = @"
                INSERT INTO notifications (user_id, user_name, type, subject, message, recipient, is_sent, sent_at, error_message, created_at) 
                VALUES (@user_id, @user_name, @type::notification_type, @subject, @message, @recipient, @is_sent, @sent_at, @error_message, @created_at)";
            
        var cmd = new NpgsqlCommand(query, conn);

       cmd.Parameters.AddWithValue("@user_id", notification.UserId);
            cmd.Parameters.AddWithValue("@user_name", notification.UserName);
            cmd.Parameters.AddWithValue("@type", notification.Type);
            cmd.Parameters.AddWithValue("@subject", notification.Subject ?? "");
            cmd.Parameters.AddWithValue("@message", notification.Message);
            cmd.Parameters.AddWithValue("@recipient", notification.Recipient);
            cmd.Parameters.AddWithValue("@is_sent", notification.IsSent);
            cmd.Parameters.AddWithValue("@sent_at", notification.SentAt);
            cmd.Parameters.AddWithValue("@error_message", notification.ErrorMessage ?? "");
            cmd.Parameters.AddWithValue("@created_at", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

/*
        public void Update(NotificationEntity notification)
        {
        var conn = _dbConnection.GetConnection();
        string query = $"update table notifications set (user_id, user_name, type, subject, Message, Recipient, Is_sent, Sent_At, ErrorMessage) values ({userId}, '{userName}', '{type}', '{subject}', '{message}', '{recipient}', {issent}, {sentat}, {errormessge})";
        var cmd = new NpgsqlCommand(query, conn);
        }

        public List<NotificationEntity> GetSentNotifications()
        {
            var notifications = new List<NotificationEntity>();

       using var conn = _dbConnection.GetConnection();
       string query = "SELECT id, user_id, user_name, type, subject, message, recipient, is_sent, sent_at, error_message, created_at FROM notifications WHERE is_sent = true ORDER BY sent_at DESC";
        var cmd = new NpgsqlCommand(query, conn);

        var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                notifications.Add(new NotificationEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    Type = reader.GetString(3),
                    Subject = reader.GetString(4),
                    Message = reader.GetString(5),
                    Recipient = reader.GetString(6),
                    IsSent = reader.GetBoolean(7),
                    SentAt = reader.GetDateTime(8),
                    ErrorMessage = reader.GetString(9)
                });
            }
            return notifications;
        }

        */
    }
}