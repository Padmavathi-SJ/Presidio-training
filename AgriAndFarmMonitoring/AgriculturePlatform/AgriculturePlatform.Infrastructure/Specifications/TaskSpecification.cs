// AgriculturePlatform.Infrastructure/Specifications/TaskSpecification.cs
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class TaskSpecification : BaseSpecification<WorkerTask>
{
    public TaskSpecification(
        int farmId,
        int? workerId,
        int? fieldId,
        int? cropCycleId,
        string? status,
        string? priority,
        string? taskName,
        DateTime? assignedDateFrom,
        DateTime? assignedDateTo,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        bool? isOverdue,
        bool? activeOnly)
    {
        // Base filter - by farm
        AddCriteria(t => t.FarmId == farmId && !t.IsDeleted);

        // Filter by worker
        if (workerId.HasValue)
        {
            AddCriteria(t => t.WorkerId == workerId.Value);
        }

        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(t => t.FieldId == fieldId.Value);
        }

        // Filter by crop cycle
        if (cropCycleId.HasValue)
        {
            AddCriteria(t => t.CropCycleId == cropCycleId.Value);
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TaskStatusEnum>(status, true, out var parsedStatus))
        {
            AddCriteria(t => t.Status == parsedStatus);
        }

        // Filter by priority
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TaskPriorityEnum>(priority, true, out var parsedPriority))
        {
            AddCriteria(t => t.Priority == parsedPriority);
        }

        // Filter by task name
        if (!string.IsNullOrWhiteSpace(taskName) && Enum.TryParse<TaskTypeEnum>(taskName, true, out var parsedTaskName))
        {
            AddCriteria(t => t.TaskName == parsedTaskName);
        }

        // Filter by assigned date range
        if (assignedDateFrom.HasValue)
        {
            AddCriteria(t => t.AssignedDate >= assignedDateFrom.Value);
        }
        if (assignedDateTo.HasValue)
        {
            AddCriteria(t => t.AssignedDate <= assignedDateTo.Value);
        }

        // Filter by due date range
        if (dueDateFrom.HasValue)
        {
            AddCriteria(t => t.DueDate >= dueDateFrom.Value);
        }
        if (dueDateTo.HasValue)
        {
            AddCriteria(t => t.DueDate <= dueDateTo.Value);
        }

        // Overdue tasks filter
        if (isOverdue.HasValue && isOverdue.Value)
        {
            AddCriteria(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatusEnum.COMPLETED);
        }

        // Active only filter (exclude completed and cancelled)
        if (activeOnly.HasValue && activeOnly.Value)
        {
            AddCriteria(t => t.Status == TaskStatusEnum.PENDING || t.Status == TaskStatusEnum.IN_PROGRESS);
        }

        // Include navigation properties
        AddInclude(t => t.Worker);
        AddInclude(t => t.Field);
        AddInclude(t => t.CropCycle);
    }
}