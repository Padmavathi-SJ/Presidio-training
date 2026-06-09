// AgriculturePlatform.Domain/Entities/CropMonitoring/AlertThreshold.cs
using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class AlertThreshold : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public string CropType { get; set; } = string.Empty;
    public string GrowthStage { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Severity { get; set; } = "MEDIUM";
    public bool IsActive { get; set; } = true;
    public string? NotificationEmails { get; set; }
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
}