using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class SensorReading
{
    public long Id { get; set; }
    public int FieldId { get; set; }
    public int CropCycleId { get; set; }
    public SensorTypeEnum? SensorType { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}