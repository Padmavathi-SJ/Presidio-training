using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Domain.Entities.WorkerManagement;

public class WorkerTask
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AdminId { get; set; }
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
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
}