using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.WorkerManagement;

public class Task
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public int? FieldId { get; set; }
    public int? CropCycleId { get; set; }
    public TaskTypeEnum? TaskName { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public TaskStatusEnum? Status { get; set; } = TaskStatusEnum.PENDING;
    public TaskPriorityEnum? Priority { get; set; } = TaskPriorityEnum.MEDIUM;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Worker? Worker { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}