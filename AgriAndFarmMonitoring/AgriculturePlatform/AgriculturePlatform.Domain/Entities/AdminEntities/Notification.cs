// Domain/Entities/Admin/Notification.cs
using System.Text.Json;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class Notification
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
}