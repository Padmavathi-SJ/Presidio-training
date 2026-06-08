// AgriculturePlatform.Application/DTOs/Task/TaskDto.cs
namespace AgriculturePlatform.Application.DTOs.Task;

public class TaskDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int? FieldId { get; set; }
    public string? FieldName { get; set; }
    public int? CropCycleId { get; set; }
    public string? CropType { get; set; }
    public string? TaskName { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
    public bool IsOverdue { get; set; }
    public int? CompletedDaysAgo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}