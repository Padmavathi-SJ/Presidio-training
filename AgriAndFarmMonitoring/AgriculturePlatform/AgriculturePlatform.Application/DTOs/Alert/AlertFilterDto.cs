// AgriculturePlatform.Application/DTOs/Alert/AlertFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class AlertFilterDto
{
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public bool? IsResolved { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool IsDescending { get; set; } = true;
}