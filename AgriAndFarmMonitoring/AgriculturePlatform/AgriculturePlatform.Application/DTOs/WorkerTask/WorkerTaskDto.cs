// AgriculturePlatform.Application/DTOs/WorkerTask/WorkerTaskDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerTask;

public class WorkerTaskDto
{
    public int Id { get; set; }
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
    public string? CompletionNotes { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DaysToComplete { get; set; }
}