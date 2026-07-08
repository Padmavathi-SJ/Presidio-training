// Application/DTOs/CropCycle/CreateCropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class CreateCropCycleDto
{
    public int FieldId { get; set; }
    public string CropType { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    
    // Optional: Allow manual stage override
    public string? GrowthStage { get; set; }
    public string? Status { get; set; } = "ACTIVE";
    public bool AutoUpdateGrowthStage { get; set; } = true;
}