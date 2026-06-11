// Application/DTOs/YieldReport/CreateYieldReportDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class CreateYieldReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = "CUSTOM"; // DAILY, WEEKLY, MONTHLY, SEASONAL, YEARLY, CUSTOM
    public int? CropCycleId { get; set; }
    public int? FieldId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsScheduled { get; set; } = false;
    public string? ScheduleCron { get; set; }
}