using System;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class ChatMessageDto
    {
        public string Query { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
