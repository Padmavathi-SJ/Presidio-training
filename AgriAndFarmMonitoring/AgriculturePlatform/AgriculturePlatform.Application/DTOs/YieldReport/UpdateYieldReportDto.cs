// Application/DTOs/YieldReport/UpdateYieldReportDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class UpdateYieldReportDto
{
    public string? ReportName { get; set; }
    public bool? IsScheduled { get; set; }
    public string? ScheduleCron { get; set; }
}