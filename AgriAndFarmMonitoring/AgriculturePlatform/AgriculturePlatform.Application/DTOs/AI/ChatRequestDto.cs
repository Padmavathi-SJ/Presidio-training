namespace AgriculturePlatform.Application.DTOs.AI
{
    public class ChatRequestDto
    {
        public string? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int FarmId { get; set; }
        public int UserId { get; set; }
    }
}
