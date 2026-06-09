// AgriculturePlatform.Application/DTOs/WorkerTask/WorkerTaskFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerTask;

public class WorkerTaskFilterDto
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? TaskName { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? IsOverdue { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "DueDate";
    public bool IsDescending { get; set; } = false;
}