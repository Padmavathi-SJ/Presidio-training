// Application/DTOs/Email/EmailDto.cs
namespace AgriculturePlatform.Application.DTOs.Email;

public class EmailDto
{
    public string To { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? Attachments { get; set; }
}

public class SensorAlertEmailDto
{
    public string FarmName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string CropType { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime AlertTime { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public string DashboardLink { get; set; } = string.Empty;
}