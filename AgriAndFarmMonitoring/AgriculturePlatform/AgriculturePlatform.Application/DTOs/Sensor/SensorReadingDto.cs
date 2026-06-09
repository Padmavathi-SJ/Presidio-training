// AgriculturePlatform.Application/DTOs/Sensor/SensorReadingDto.cs
namespace AgriculturePlatform.Application.DTOs.Sensor;

public class SensorReadingDto
{
    public long Id { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int CropCycleId { get; set; }
    public string? CropType { get; set; }
    public string? SensorType { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool IsThresholdViolation { get; set; }
    public string? AlertType { get; set; }
}