// AgriculturePlatform.Application/DTOs/Weather/WeatherStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherStatisticsDto
{
    public int TotalRecords { get; set; }
    public int FieldsWithData { get; set; }
    public double AverageTemperature { get; set; }
    public double AverageHumidity { get; set; }
    public double TotalRainfall { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public DateTime LastUpdated { get; set; }
}