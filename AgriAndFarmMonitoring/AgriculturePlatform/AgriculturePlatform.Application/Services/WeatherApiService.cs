// AgriculturePlatform.Application/Services/WeatherApiService.cs
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.Application.Services;

public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public WeatherApiService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["WeatherApi:ApiKey"] ?? "your-api-key";
        _baseUrl = configuration["WeatherApi:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
    }

    public async Task<CurrentWeatherDto> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var url = $"{_baseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        
        try
        {
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Weather API error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {json}"); // Debug log
            
            // Use JsonDocument for more flexible parsing
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            // Parse main data
            double temperature = 0;
            double humidity = 0;
            if (root.TryGetProperty("main", out var mainElement))
            {
                if (mainElement.TryGetProperty("temp", out var tempElement))
                    temperature = tempElement.GetDouble();
                if (mainElement.TryGetProperty("humidity", out var humidityElement))
                    humidity = humidityElement.GetDouble();
            }
            
            // Parse wind speed
            double windSpeed = 0;
            if (root.TryGetProperty("wind", out var windElement))
            {
                if (windElement.TryGetProperty("speed", out var speedElement))
                    windSpeed = speedElement.GetDouble();
            }
            
            // Parse weather condition
            string condition = "Clear";
            if (root.TryGetProperty("weather", out var weatherArray) && weatherArray.GetArrayLength() > 0)
            {
                var firstWeather = weatherArray[0];
                if (firstWeather.TryGetProperty("main", out var mainCondition))
                    condition = mainCondition.GetString() ?? "Clear";
            }

            return new CurrentWeatherDto
            {
                Temperature = temperature,
                Humidity = humidity,
                WindSpeed = windSpeed,
                Condition = MapCondition(condition),
                ObservedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            throw new Exception($"JSON parsing error: {ex.Message}");
        }
    }

    public async Task<WeatherForecastDto> GetWeatherForecastAsync(double latitude, double longitude)
    {
        var url = $"{_baseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Weather API error: {response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        
        var forecast = new WeatherForecastDto();
        var dailyForecasts = new Dictionary<string, List<dynamic>>();

        if (root.TryGetProperty("list", out var listElement))
        {
            var groupedData = new Dictionary<string, List<DailyForecastData>>();
            
            foreach (var item in listElement.EnumerateArray())
            {
                var dtTxt = item.GetProperty("dt_txt").GetString();
                var date = dtTxt?.Split(' ')[0] ?? "";
                
                if (!groupedData.ContainsKey(date))
                    groupedData[date] = new List<DailyForecastData>();
                
                var main = item.GetProperty("main");
                var wind = item.GetProperty("wind");
                var weather = item.GetProperty("weather")[0];
                
                groupedData[date].Add(new DailyForecastData
                {
                    Temp = main.GetProperty("temp").GetDouble(),
                    Humidity = main.GetProperty("humidity").GetDouble(),
                    WindSpeed = wind.GetProperty("speed").GetDouble(),
                    Condition = weather.GetProperty("main").GetString() ?? "Clear",
                    Pop = item.TryGetProperty("pop", out var pop) ? pop.GetDouble() : 0
                });
            }
            
            foreach (var group in groupedData)
            {
                var dayData = group.Value;
                forecast.DailyForecasts.Add(new DailyForecastDto
                {
                    Date = DateTime.Parse(group.Key),
                    MaxTemp = dayData.Max(d => d.Temp),
                    MinTemp = dayData.Min(d => d.Temp),
                    Condition = MapCondition(dayData.First().Condition),
                    ChanceOfRain = dayData.Average(d => d.Pop) * 100,
                    Humidity = dayData.Average(d => d.Humidity),
                    WindSpeed = dayData.Average(d => d.WindSpeed)
                });
            }
        }

        return forecast;
    }

    public async Task<List<WeatherAlertDto>> GetWeatherAlertsAsync(double latitude, double longitude)
    {
        var alerts = new List<WeatherAlertDto>();
        
        var current = await GetCurrentWeatherAsync(latitude, longitude);
        
        if (current.Temperature > 35)
        {
            alerts.Add(new WeatherAlertDto
            {
                AlertType = "HEAT_WAVE",
                Severity = "HIGH",
                Message = $"Extreme heat warning: Temperature is {current.Temperature}°C",
                AlertTime = DateTime.UtcNow
            });
        }
        
        if (current.Temperature < 0)
        {
            alerts.Add(new WeatherAlertDto
            {
                AlertType = "FROST",
                Severity = "HIGH",
                Message = $"Frost warning: Temperature is {current.Temperature}°C",
                AlertTime = DateTime.UtcNow
            });
        }
        
        if (current.WindSpeed > 50)
        {
            alerts.Add(new WeatherAlertDto
            {
                AlertType = "STORM",
                Severity = "HIGH",
                Message = $"Storm warning: Wind speed is {current.WindSpeed} km/h",
                AlertTime = DateTime.UtcNow
            });
        }
        
        return alerts;
    }

    private string MapCondition(string condition)
    {
        return condition switch
        {
            "Clear" => "CLEAR",
            "Clouds" => "CLOUDY",
            "Rain" => "RAINY",
            "Drizzle" => "RAINY",
            "Thunderstorm" => "STORMY",
            "Snow" => "SNOWY",
            "Mist" or "Fog" => "FOGGY",
            _ => "CLEAR"
        };
    }

    private class DailyForecastData
    {
        public double Temp { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string Condition { get; set; } = string.Empty;
        public double Pop { get; set; }
    }
}