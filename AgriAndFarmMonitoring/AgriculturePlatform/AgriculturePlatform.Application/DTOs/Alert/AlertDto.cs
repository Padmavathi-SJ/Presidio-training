// AgriculturePlatform.Application/DTOs/Alert/AlertDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class AlertDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int? CropCycleId { get; set; }
    public string? CropType { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? Message { get; set; }
    public bool IsResolved { get; set; }
    public decimal? SensorValue { get; set; }
    public decimal? ThresholdValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}