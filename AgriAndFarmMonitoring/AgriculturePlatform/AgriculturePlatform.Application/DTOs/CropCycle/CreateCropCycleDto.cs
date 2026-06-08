// AgriculturePlatform.Application/DTOs/CropCycle/CreateCropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class CreateCropCycleDto
{
    public int FieldId { get; set; }
    public string CropType { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public string? GrowthStage { get; set; } = "GERMINATION";
    public string? Status { get; set; } = "ACTIVE";
}