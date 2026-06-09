// AgriculturePlatform.Application/DTOs/Observation/CreateObservationDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class CreateObservationDto
{
    public int FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public DateTime ObservationDate { get; set; }
    public string? CropHealth { get; set; }
    public bool PestDetected { get; set; } = false;
    public string? PestType { get; set; }
    public string? Notes { get; set; }
    public List<string>? ImageUrls { get; set; }
}