using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Alert
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public AlertTypeEnum? AlertType { get; set; }
    public AlertSeverityEnum? Severity { get; set; }
    public string? Message { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation properties
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}