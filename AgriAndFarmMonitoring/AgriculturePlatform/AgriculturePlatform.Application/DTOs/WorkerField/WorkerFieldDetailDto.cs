// AgriculturePlatform.Application/DTOs/WorkerField/WorkerFieldDetailDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerField;

public class WorkerFieldDetailDto
{
    public int AssignmentId { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? FieldLocation { get; set; }
    public decimal? FieldAreaHectares { get; set; }
    public string? FieldSoilType { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string? Notes { get; set; }
    
    // Crop cycles in this field
    public List<WorkerFieldCropCycleDto> CropCycles { get; set; } = new();
}

public class WorkerFieldCropCycleDto
{
    public int Id { get; set; }
    public string? CropType { get; set; }
    public DateTime? PlantingDate { get; set; }
    public DateTime? ExpectedHarvestDate { get; set; }
    public string? GrowthStage { get; set; }
    public string? Status { get; set; }
}