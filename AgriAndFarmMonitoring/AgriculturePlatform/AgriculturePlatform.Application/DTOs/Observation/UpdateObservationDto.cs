// Application/DTOs/Observation/UpdateObservationDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class UpdateObservationDto
{
    public DateTime? ObservationDate { get; set; }
    public string? CropHealth { get; set; }
    public bool? PestDetected { get; set; }
    public string? PestType { get; set; }
    public string? Notes { get; set; }
    public List<string>? ImageUrls { get; set; }
}