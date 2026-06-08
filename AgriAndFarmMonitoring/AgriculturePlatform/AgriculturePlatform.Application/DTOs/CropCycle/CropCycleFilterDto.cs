// AgriculturePlatform.Application/DTOs/CropCycle/CropCycleFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class CropCycleFilterDto
{
    public int? FieldId { get; set; }
    public string? CropType { get; set; }
    public string? GrowthStage { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpectedHarvestDateFrom { get; set; }
    public DateTime? ExpectedHarvestDateTo { get; set; }
    public bool? IncludeDeleted { get; set; } = false;
    public bool? ActiveOnly { get; set; } = false;
    public bool? OverdueOnly { get; set; } = false;
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
}