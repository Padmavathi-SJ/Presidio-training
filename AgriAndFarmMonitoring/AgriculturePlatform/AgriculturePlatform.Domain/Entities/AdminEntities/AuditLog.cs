using System.Text.Json;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;
namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class AuditLog : BaseEntity
{

    public int? FarmId { get; set; }
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public JsonDocument? OldValue { get; set; }
    public JsonDocument? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
}