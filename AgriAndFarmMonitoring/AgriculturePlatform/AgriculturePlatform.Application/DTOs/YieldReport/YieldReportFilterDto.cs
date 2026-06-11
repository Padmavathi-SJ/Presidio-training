// Application/DTOs/YieldReport/YieldReportFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class YieldReportFilterDto
{
    public int? CropCycleId { get; set; }
    public int? FieldId { get; set; }
    public string? ReportType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsScheduled { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
}