// AgriculturePlatform.Application/DTOs/Worker/WorkerFieldDetailDto.cs
namespace AgriculturePlatform.Application.DTOs.Worker;

public class WorkerFieldDetailDto
{
    public int AssignmentId { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Crop cycles in this field
    public List<WorkerCropCycleDto> CropCycles { get; set; } = new();
}