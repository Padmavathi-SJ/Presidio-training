// AgriculturePlatform.Application/DTOs/CropCycle/CropCycleDto.cs
namespace AgriculturePlatform.Application.DTOs.CropCycle;

public class CropCycleDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public string? GrowthStage { get; set; }
    public string? Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}