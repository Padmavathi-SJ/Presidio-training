// AgriculturePlatform.Application/DTOs/WorkerField/WorkerFieldFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerField;

public class WorkerFieldFilterDto
{
    public int? WorkerId { get; set; }
    public int? FieldId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? AssignedDateFrom { get; set; }
    public DateTime? AssignedDateTo { get; set; }
    // ✅ Add EndDate filters
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "AssignedDate";
    public bool IsDescending { get; set; } = true;
}