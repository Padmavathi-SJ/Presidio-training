// AgriculturePlatform.Application/DTOs/Observation/ObservationFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Observation;

public class ObservationFilterDto
{
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public int? WorkerId { get; set; }
    public string? CropHealth { get; set; }
    public bool? PestDetected { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IncludeDeleted { get; set; } = false;
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ObservationDate";
    public bool IsDescending { get; set; } = true;
}