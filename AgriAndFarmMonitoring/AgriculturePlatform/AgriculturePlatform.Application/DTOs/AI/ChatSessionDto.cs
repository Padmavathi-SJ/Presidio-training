using System;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class ChatSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Snippet { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
