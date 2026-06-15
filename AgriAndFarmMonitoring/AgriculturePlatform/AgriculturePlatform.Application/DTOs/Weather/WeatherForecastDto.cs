// AgriculturePlatform.Application/DTOs/Weather/WeatherForecastDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherForecastDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public List<DailyForecastDto> DailyForecasts { get; set; } = new();
    public CurrentWeatherDto CurrentWeather { get; set; } = new();
}

public class DailyForecastDto
{
    public DateTime Date { get; set; }
    public double MaxTemp { get; set; }      // Changed from decimal
    public double MinTemp { get; set; }      // Changed from decimal
    public string Condition { get; set; } = string.Empty;
    public double ChanceOfRain { get; set; }  // Changed from decimal
    public double? RainfallMm { get; set; }   // Changed from decimal
    public double Humidity { get; set; }      // Changed from decimal
    public double WindSpeed { get; set; }     // Changed from decimal
    public string? Alert { get; set; }
}

