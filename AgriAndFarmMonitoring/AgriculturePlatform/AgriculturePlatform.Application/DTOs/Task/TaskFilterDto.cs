// AgriculturePlatform.Application/DTOs/Task/TaskFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class TaskFilterDto
{
    public int? WorkerId { get; set; }
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? TaskName { get; set; }
    public DateTime? AssignedDateFrom { get; set; }
    public DateTime? AssignedDateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? ActiveOnly { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "AssignedDate";
    public bool IsDescending { get; set; } = true;
}