// AgriculturePlatform.Application/DTOs/Weather/WeatherDataDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherDataDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public double? Temperature { get; set; }   // Changed from decimal
    public double? Humidity { get; set; }      // Changed from decimal
    public double? RainfallMm { get; set; }    // Changed from decimal
    public double? WindSpeed { get; set; }     // Changed from decimal
    public string? Condition { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}