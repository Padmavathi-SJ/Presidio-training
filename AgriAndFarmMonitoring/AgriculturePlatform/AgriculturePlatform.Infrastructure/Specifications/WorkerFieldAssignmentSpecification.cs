// AgriculturePlatform.Infrastructure/Specifications/WorkerFieldAssignmentSpecification.cs
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class WorkerFieldAssignmentSpecification : BaseSpecification<WorkerFieldAssignment>
{
    public WorkerFieldAssignmentSpecification(
        int farmId,
        int? workerId,
        int? fieldId,
        bool? isActive,
        DateTime? assignedDateFrom,
        DateTime? assignedDateTo,
        bool includeDeleted = false)
    {
        // Base filter - by farm
        AddCriteria(a => a.FarmId == farmId);

        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(a => !a.IsDeleted);
        }

        // Filter by worker
        if (workerId.HasValue)
        {
            AddCriteria(a => a.WorkerId == workerId.Value);
        }

        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(a => a.FieldId == fieldId.Value);
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            AddCriteria(a => a.IsActive == isActive.Value);
        }

        // Filter by assigned date range
        if (assignedDateFrom.HasValue)
        {
            AddCriteria(a => a.AssignedDate >= assignedDateFrom.Value);
        }
        if (assignedDateTo.HasValue)
        {
            AddCriteria(a => a.AssignedDate <= assignedDateTo.Value);
        }

        // Include navigation properties
        AddInclude(a => a.Worker);
        AddInclude(a => a.Field);
        AddInclude(a => a.Farm);
    }
}