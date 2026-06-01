using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class WeatherData
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? RainfallMm { get; set; }
    public decimal? WindSpeed { get; set; }
    public WeatherConditionEnum? Condition { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Field? Field { get; set; }
}