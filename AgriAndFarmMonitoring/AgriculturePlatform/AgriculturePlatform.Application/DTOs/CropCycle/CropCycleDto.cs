// Application/DTOs/CropCycle/CropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class CropCycleDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public DateTime? ActualHarvestDate { get; set; }
    public string? GrowthStage { get; set; }
    public string? PreviousGrowthStage { get; set; }
    public DateTime? LastStageUpdate { get; set; }
    public string? Status { get; set; }
    public bool AutoUpdateGrowthStage { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // ✅ Computed properties (not from DB)
    public double? GrowthPercentage { get; set; }
    public int? DaysUntilHarvest { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsReadyForHarvest { get; set; }
}