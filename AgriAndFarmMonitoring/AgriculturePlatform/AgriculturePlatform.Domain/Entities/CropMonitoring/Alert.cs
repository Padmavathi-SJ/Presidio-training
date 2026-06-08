using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Alert : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public AlertTypeEnum? AlertType { get; set; }
    public AlertSeverityEnum? Severity { get; set; }
    public string? Message { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}