// AgriculturePlatform.Application/Services/WeatherApiService.cs
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.Application.Services;

public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ILogger<WeatherApiService> _logger;

    public WeatherApiService(IConfiguration configuration, ILogger<WeatherApiService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _apiKey = configuration["WeatherApi:ApiKey"] ?? throw new Exception("Weather API Key not configured in appsettings.json");
        _baseUrl = configuration["WeatherApi:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
        _logger = logger;
        
        _logger.LogInformation("WeatherApiService initialized with BaseUrl: {BaseUrl}", _baseUrl);
    }

    public async Task<CurrentWeatherDto> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var url = $"{_baseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        
        try
        {
            _logger.LogDebug("Fetching current weather for lat: {Lat}, lon: {Lon}", latitude, longitude);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Weather API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Weather API error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Weather API response received");
            
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
            
            // Parse rainfall
            double? rainfallMm = null;
            if (root.TryGetProperty("rain", out var rainElement))
            {
                if (rainElement.TryGetProperty("1h", out var rain1h))
                    rainfallMm = rain1h.GetDouble();
                else if (rainElement.TryGetProperty("3h", out var rain3h))
                    rainfallMm = rain3h.GetDouble();
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
                Temperature = Math.Round(temperature, 1),
                Humidity = Math.Round(humidity, 1),
                WindSpeed = Math.Round(windSpeed, 1),
                RainfallMm = rainfallMm.HasValue ? Math.Round(rainfallMm.Value, 1) : null,
                Condition = MapCondition(condition),
                ObservedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching weather data");
            throw new Exception($"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error");
            throw new Exception($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather data");
            throw;
        }
    }

    public async Task<WeatherForecastDto> GetWeatherForecastAsync(double latitude, double longitude)
    {
        var url = $"{_baseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
        
        try
        {
            _logger.LogDebug("Fetching forecast for lat: {Lat}, lon: {Lon}", latitude, longitude);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Weather API forecast error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Weather API forecast error ({response.StatusCode}): {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Weather forecast response received");
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            var forecast = new WeatherForecastDto();
            var groupedData = new Dictionary<string, List<DailyForecastData>>();

            if (root.TryGetProperty("list", out var listElement))
            {
                foreach (var item in listElement.EnumerateArray())
                {
                    var dtTxt = item.GetProperty("dt_txt").GetString();
                    var date = dtTxt?.Split(' ')[0] ?? "";
                    
                    if (!groupedData.ContainsKey(date))
                        groupedData[date] = new List<DailyForecastData>();
                    
                    var main = item.GetProperty("main");
                    var wind = item.GetProperty("wind");
                    var weather = item.GetProperty("weather")[0];
                    
                    double? rainfall = null;
                    if (item.TryGetProperty("rain", out var rainElement))
                    {
                        if (rainElement.TryGetProperty("3h", out var rain3h))
                            rainfall = rain3h.GetDouble();
                    }
                    
                    groupedData[date].Add(new DailyForecastData
                    {
                        Temp = main.GetProperty("temp").GetDouble(),
                        Humidity = main.GetProperty("humidity").GetDouble(),
                        WindSpeed = wind.GetProperty("speed").GetDouble(),
                        Condition = weather.GetProperty("main").GetString() ?? "Clear",
                        Pop = item.TryGetProperty("pop", out var pop) ? pop.GetDouble() : 0,
                        Rainfall = rainfall
                    });
                }
                
                foreach (var group in groupedData)
                {
                    var dayData = group.Value;
                    forecast.DailyForecasts.Add(new DailyForecastDto
                    {
                        Date = DateTime.Parse(group.Key),
                        MaxTemp = Math.Round(dayData.Max(d => d.Temp), 1),
                        MinTemp = Math.Round(dayData.Min(d => d.Temp), 1),
                        Condition = MapCondition(dayData.First().Condition),
                        ChanceOfRain = Math.Round(dayData.Average(d => d.Pop) * 100, 0),
                        Humidity = Math.Round(dayData.Average(d => d.Humidity), 1),
                        WindSpeed = Math.Round(dayData.Average(d => d.WindSpeed), 1),
                        RainfallMm = dayData.Any(d => d.Rainfall.HasValue) 
                            ? Math.Round(dayData.Where(d => d.Rainfall.HasValue).Average(d => d.Rainfall!.Value), 1) 
                            : null
                    });
                }
            }

            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather forecast");
            throw;
        }
    }

    public async Task<List<WeatherAlertDto>> GetWeatherAlertsAsync(double latitude, double longitude)
    {
        var alerts = new List<WeatherAlertDto>();
        
        try
        {
            var current = await GetCurrentWeatherAsync(latitude, longitude);
            
            // Heat wave alert
            if (current.Temperature > 35)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "HEAT_WAVE",
                    Severity = "WARNING",
                    Title = "High Temperature Alert",
                    Message = $"Temperature reached {current.Temperature}°C which exceeds safe threshold of 35°C",
                    Temperature = current.Temperature,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            else if (current.Temperature > 30)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "HEAT_WAVE",
                    Severity = "ADVISORY",
                    Title = "Moderate Temperature Alert",
                    Message = $"Temperature is {current.Temperature}°C. Monitor for potential heat stress.",
                    Temperature = current.Temperature,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(12)
                });
            }
            
            // Frost alert
            if (current.Temperature < 2)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "FROST",
                    Severity = "WARNING",
                    Title = "Frost Warning",
                    Message = $"Temperature dropped to {current.Temperature}°C. Frost risk detected.",
                    Temperature = current.Temperature,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(12)
                });
            }
            
            // Storm alert
            if (current.WindSpeed > 50)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "STORM",
                    Severity = "WARNING",
                    Title = "High Wind Alert",
                    Message = $"Wind speed is {current.WindSpeed} km/h. Storm conditions detected.",
                    WindSpeed = current.WindSpeed,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(6)
                });
            }
            else if (current.WindSpeed > 30)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "HIGH_WIND",
                    Severity = "ADVISORY",
                    Title = "Moderate Wind Alert",
                    Message = $"Wind speed is {current.WindSpeed} km/h. Monitor for potential issues.",
                    WindSpeed = current.WindSpeed,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(6)
                });
            }
            
            // Heavy rain alert
            if (current.RainfallMm.HasValue && current.RainfallMm.Value > 30)
            {
                alerts.Add(new WeatherAlertDto
                {
                    AlertType = "HEAVY_RAIN",
                    Severity = "WARNING",
                    Title = "Heavy Rainfall Alert",
                    Message = $"Rainfall of {current.RainfallMm.Value}mm detected. Risk of flooding.",
                    RainfallMm = current.RainfallMm,
                    AlertTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(12)
                });
            }
            
            _logger.LogInformation("Generated {Count} weather alerts", alerts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weather alerts");
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
        public double? Rainfall { get; set; }
    }
}