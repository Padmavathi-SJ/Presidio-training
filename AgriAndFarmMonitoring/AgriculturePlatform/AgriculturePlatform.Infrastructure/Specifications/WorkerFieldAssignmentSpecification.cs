// AgriculturePlatform.Infrastructure/Specifications/WorkerFieldAssignmentSpecification.cs
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class WorkerFieldAssignmentSpecification : BaseSpecification<WorkerFieldAssignment>
{
    public WorkerFieldAssignmentSpecification(
        int farmId,
        int? workerId = null,
        int? fieldId = null,
        bool? isActive = null,
        DateTime? assignedDateFrom = null,
        DateTime? assignedDateTo = null,
        DateTime? endDateFrom = null,      // ✅ Added
        DateTime? endDateTo = null,        // ✅ Added
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
            var fromDate = assignedDateFrom.Value.Date.ToUniversalTime();
            AddCriteria(a => a.AssignedDate >= fromDate);
        }
        
        if (assignedDateTo.HasValue)
        {
            var toDate = assignedDateTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
            AddCriteria(a => a.AssignedDate <= toDate);
        }

        // ✅ Filter by end date range
        if (endDateFrom.HasValue)
        {
            var fromDate = endDateFrom.Value.Date.ToUniversalTime();
            AddCriteria(a => a.EndDate >= fromDate);
        }
        
        if (endDateTo.HasValue)
        {
            var toDate = endDateTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
            AddCriteria(a => a.EndDate <= toDate);
        }

        // Include navigation
        AddInclude(a => a.Worker);
        AddInclude(a => a.Field);
        AddInclude(a => a.Farm);
        AddInclude(a => a.Admin);
    }
}