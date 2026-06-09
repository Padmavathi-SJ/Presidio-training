// AgriculturePlatform.Application/DTOs/Alert/AlertThresholdDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class AlertThresholdDto
{
    public int Id { get; set; }
    public string CropType { get; set; } = string.Empty;
    public string GrowthStage { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? NotificationEmails { get; set; }
}

public class CreateAlertThresholdDto
{
    public string CropType { get; set; } = string.Empty;
    public string GrowthStage { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Severity { get; set; } = "MEDIUM";
    public string? NotificationEmails { get; set; }
}