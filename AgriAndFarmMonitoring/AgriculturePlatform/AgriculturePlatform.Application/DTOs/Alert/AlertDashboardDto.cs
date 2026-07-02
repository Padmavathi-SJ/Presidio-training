// AgriculturePlatform.Application/DTOs/Alert/AlertDashboardDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class AlertDashboardDto
{
    public int TotalAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int HighAlerts { get; set; }
    public int MediumAlerts { get; set; }
    public int LowAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int UnresolvedAlerts { get; set; }
    public Dictionary<string, int> AlertsByField { get; set; } = new();
    public Dictionary<string, int> AlertsByType { get; set; } = new();
    public List<RecentAlertDto> RecentAlerts { get; set; } = new();
}

public class RecentAlertDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}