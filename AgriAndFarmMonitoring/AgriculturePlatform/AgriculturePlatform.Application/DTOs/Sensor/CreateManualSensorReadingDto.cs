// AgriculturePlatform.Application/DTOs/Sensor/CreateManualSensorReadingDto.cs
using System.ComponentModel.DataAnnotations;

namespace AgriculturePlatform.Application.DTOs.Sensor;

public class CreateManualSensorReadingDto
{
    [Required]
    public int FieldId { get; set; }

    [Required]
    public int CropCycleId { get; set; }

    [Required]
    public string SensorType { get; set; } = string.Empty;

    [Required]
    public decimal Value { get; set; }

    [Required]
    public string Unit { get; set; } = string.Empty;

    public DateTime? RecordedAt { get; set; }
}
