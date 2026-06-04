using System.Text.Json;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class AuditLog
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public JsonDocument? OldValue { get; set; }
    public JsonDocument? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
}