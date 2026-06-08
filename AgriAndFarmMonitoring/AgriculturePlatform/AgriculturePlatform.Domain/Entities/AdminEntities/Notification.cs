using System.Text.Json;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class Notification : BaseEntity
{
    public int? FarmId { get; set; }
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
}