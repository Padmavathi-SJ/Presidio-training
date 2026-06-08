// AgriculturePlatform.Application/DTOs/CropCycle/UpdateCropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class UpdateCropCycleDto
{
    public string? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public string? GrowthStage { get; set; }
    public string? Status { get; set; }
}