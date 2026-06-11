// Application/DTOs/QualityCheck/QualityCheckFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.QualityCheck;

public class QualityCheckFilterDto
{
    public int? HarvestId { get; set; }
    public int? WorkerId { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? FinalGrade { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IncludeDeleted { get; set; } = false;
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CheckDate";
    public bool IsDescending { get; set; } = true;
}