// AgriculturePlatform.Application/DTOs/Sensor/SensorReadingFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Sensor;

public class SensorReadingFilterDto
{
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string? SensorType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? LatestOnly { get; set; }
    public string? GroupBy { get; set; } // Day, Week, Month
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "RecordedAt";
    public bool IsDescending { get; set; } = true;
}