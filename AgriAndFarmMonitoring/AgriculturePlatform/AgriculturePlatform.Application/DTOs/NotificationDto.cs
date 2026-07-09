using System.Text.Json;

namespace AgriculturePlatform.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int? FarmId { get; set; }
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
