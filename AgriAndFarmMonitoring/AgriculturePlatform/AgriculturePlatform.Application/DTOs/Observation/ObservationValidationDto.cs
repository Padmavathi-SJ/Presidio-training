// Application/DTOs/Observation/ObservationValidationDto.cs (NEW FILE)
namespace AgriculturePlatform.Application.DTOs.Observation;

public class ObservationValidationDto
{
    public int ObservationId { get; set; }
    public string ValidationStatus { get; set; } = "verified"; // verified, questioned, invalid
    public string? AdminNotes { get; set; }
    public string? FlagReason { get; set; } // outlier, inconsistent_data, missing_info, duplicate
}

public class ObservationWorkerResponseDto
{
    public int ObservationId { get; set; }
    public string WorkerResponse { get; set; } = string.Empty;
}