// AgriculturePlatform.Domain/Entities/CropMonitoring/WeatherData.cs
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class WeatherData : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public double? Temperature { get; set; }   // Changed from decimal to double
    public double? Humidity { get; set; }      // Changed from decimal to double
    public double? RainfallMm { get; set; }    // Changed from decimal to double
    public double? WindSpeed { get; set; }     // Changed from decimal to double
    public WeatherConditionEnum? Condition { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
}