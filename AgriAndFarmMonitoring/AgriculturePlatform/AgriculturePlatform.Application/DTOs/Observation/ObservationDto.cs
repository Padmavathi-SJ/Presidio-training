// Application/DTOs/Observation/ObservationDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class ObservationDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int? CropCycleId { get; set; }
    public string? CropType { get; set; }
    public int? WorkerId { get; set; }
    public string? WorkerName { get; set; }
    public DateTime ObservationDate { get; set; }
    public string? CropHealth { get; set; }
    public bool PestDetected { get; set; }
    public string? PestType { get; set; }
    public string? Notes { get; set; }
    public List<string>? ImageUrls { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // ===== NEW VALIDATION FIELDS =====
    public string ValidationStatus { get; set; } = "pending";
    public string? AdminNotes { get; set; }
    public string? WorkerResponse { get; set; }
    public int? ValidatedBy { get; set; }
    public string? ValidatorName { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public string? FlagReason { get; set; }
    
    // Computed properties for UI
    public bool IsPending => ValidationStatus == "pending";
    public bool IsVerified => ValidationStatus == "verified";
    public bool IsQuestioned => ValidationStatus == "questioned";
    public bool IsInvalid => ValidationStatus == "invalid";
    public string StatusBadgeColor => ValidationStatus switch
    {
        "verified" => "green",
        "pending" => "yellow",
        "questioned" => "orange",
        "invalid" => "red",
        _ => "gray"
    };
}