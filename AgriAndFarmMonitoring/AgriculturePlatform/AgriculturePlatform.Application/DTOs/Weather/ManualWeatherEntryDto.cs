// AgriculturePlatform.Application/DTOs/Weather/ManualWeatherEntryDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class ManualWeatherEntryDto
{
    public int FieldId { get; set; }
    public double? Temperature { get; set; }   // Changed from decimal
    public double? Humidity { get; set; }      // Changed from decimal
    public double? RainfallMm { get; set; }    // Changed from decimal
    public double? WindSpeed { get; set; }     // Changed from decimal
    public string? Condition { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
}