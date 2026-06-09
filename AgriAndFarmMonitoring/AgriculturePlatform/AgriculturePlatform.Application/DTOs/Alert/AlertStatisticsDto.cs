// AgriculturePlatform.Application/DTOs/Alert/AlertStatisticsDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class AlertStatisticsDto
{
    public int TotalAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int UnresolvedAlerts { get; set; }
    public Dictionary<string, int> AlertsByType { get; set; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    public Dictionary<string, int> AlertsByField { get; set; } = new();
    public List<AlertTrendDto> RecentTrend { get; set; } = new();
}

public class AlertTrendDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}