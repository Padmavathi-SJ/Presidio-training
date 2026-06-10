// Application/DTOs/Harvest/HarvestFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Harvest;

public class HarvestFilterDto
{
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public int? WorkerId { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? QualityGrade { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IncludeDeleted { get; set; } = false;
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "HarvestDate";
    public bool IsDescending { get; set; } = true;
}