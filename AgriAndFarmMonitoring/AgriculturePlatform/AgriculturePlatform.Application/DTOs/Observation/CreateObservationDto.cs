// Application/DTOs/Observation/CreateObservationDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class CreateObservationDto
{
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public DateTime ObservationDate { get; set; }
    public string? CropHealth { get; set; }
    public string? PestType { get; set; }
    public string? Notes { get; set; }
    
    // Image fields
    public string? ImagePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ImageCaption { get; set; }
    public List<string>? AdditionalImagePaths { get; set; }
    public string? ImageMetadata { get; set; }
}