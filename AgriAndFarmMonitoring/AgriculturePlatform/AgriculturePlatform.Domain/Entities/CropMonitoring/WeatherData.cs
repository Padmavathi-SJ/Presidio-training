// Domain/Entities/CropMonitoring/WeatherData.cs
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class WeatherData
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? RainfallMm { get; set; }
    public decimal? WindSpeed { get; set; }
    public WeatherConditionEnum? Condition { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
}