// AgriculturePlatform.Application/DTOs/Sensor/SensorStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Sensor;

public class SensorStatisticsDto
{
    public string Period { get; set; } = string.Empty;
    public Dictionary<string, DailySensorStats> DailyStats { get; set; } = new();
    public Dictionary<string, WeeklySensorStats> WeeklyStats { get; set; } = new();
    public Dictionary<string, MonthlySensorStats> MonthlyStats { get; set; } = new();
}

public class DailySensorStats
{
    public DateTime Date { get; set; }
    public decimal? AvgSoilMoisture { get; set; }
    public decimal? AvgSoilTemp { get; set; }
    public decimal? AvgAirTemp { get; set; }
    public decimal? AvgHumidity { get; set; }
    public int ReadingsCount { get; set; }
    public int AlertCount { get; set; }
}

public class WeeklySensorStats
{
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public decimal? AvgSoilMoisture { get; set; }
    public decimal? AvgSoilTemp { get; set; }
    public int AlertCount { get; set; }
}

public class MonthlySensorStats
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal? AvgSoilMoisture { get; set; }
    public decimal? AvgSoilTemp { get; set; }
    public int AlertCount { get; set; }
}