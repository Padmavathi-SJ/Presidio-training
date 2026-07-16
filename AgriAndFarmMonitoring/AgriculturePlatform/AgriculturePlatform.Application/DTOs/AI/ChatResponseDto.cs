using System;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class ChatResponseDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
