using System;

namespace NotificationSystem.DataAccess.Entities  // Correct namespace
{

    public class NotificationEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}