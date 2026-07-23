using System;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
