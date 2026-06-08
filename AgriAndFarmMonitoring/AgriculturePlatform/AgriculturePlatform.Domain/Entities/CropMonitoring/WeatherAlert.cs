// AgriculturePlatform.Domain/Entities/CropMonitoring/WeatherAlert.cs
using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class WeatherAlert : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public int FieldId { get; set; }
    public WeatherAlertTypeEnum AlertType { get; set; }        // Changed to enum
    public WeatherAlertSeverityEnum Severity { get; set; }     // Changed to enum
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? Temperature { get; set; }
    public double? WindSpeed { get; set; }
    public double? RainfallMm { get; set; }
    public bool IsAcknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public int? AcknowledgedBy { get; set; }
    public DateTime AlertTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Field? Field { get; set; }
    public virtual Admin? Acknowledger { get; set; }
}