// AgriculturePlatform.Infrastructure/Specifications/WorkerSpecification.cs
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class WorkerSpecification : BaseSpecification<Worker>
{
    public WorkerSpecification(
        int farmId,
        string? name,
        string? email,
        string? role,
        bool? isActive,
        DateTime? hireDateFrom,
        DateTime? hireDateTo,
        bool includeDeleted)
    {
        // Base filter - by farm
        AddCriteria(w => w.FarmId == farmId);

        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(w => !w.IsDeleted);
        }

        // Filter by name
        if (!string.IsNullOrWhiteSpace(name))
        {
            AddCriteria(w => w.Name.Contains(name));
        }

        // Filter by email
        if (!string.IsNullOrWhiteSpace(email))
        {
            AddCriteria(w => w.Email.Contains(email));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(role))
        {
            AddCriteria(w => w.Role != null && w.Role.ToUpper() == role.ToUpper());
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            AddCriteria(w => w.IsActive == isActive.Value);
        }

        // Filter by hire date range
        if (hireDateFrom.HasValue)
        {
            AddCriteria(w => w.HireDate >= hireDateFrom.Value);
        }
        if (hireDateTo.HasValue)
        {
            AddCriteria(w => w.HireDate <= hireDateTo.Value);
        }

        // Include navigation
        AddInclude(w => w.Farm);
        AddInclude(w => w.Admin);
    }
}