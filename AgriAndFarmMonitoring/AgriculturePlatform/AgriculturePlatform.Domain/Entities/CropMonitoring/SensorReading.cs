using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class SensorReading : BaseEntity
{
    
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public int CropCycleId { get; set; }
    public SensorTypeEnum? SensorType { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}