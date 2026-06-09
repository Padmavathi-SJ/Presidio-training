// AgriculturePlatform.Infrastructure/Specifications/ObservationSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class ObservationSpecification : BaseSpecification<Observation>
{
    public ObservationSpecification(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        int? workerId,
        string? cropHealth,
        bool? pestDetected,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted)
    {
        // Base filter - by farm
        AddCriteria(o => o.FarmId == farmId);
        
        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(o => !o.IsDeleted);
        }
        
        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(o => o.FieldId == fieldId.Value);
        }
        
        // Filter by crop cycle
        if (cropCycleId.HasValue)
        {
            AddCriteria(o => o.CropCycleId == cropCycleId.Value);
        }
        
        // Filter by worker
        if (workerId.HasValue)
        {
            AddCriteria(o => o.WorkerId == workerId.Value);
        }
        
        // Filter by crop health
        if (!string.IsNullOrWhiteSpace(cropHealth) && 
            Enum.TryParse<CropHealthEnum>(cropHealth, true, out var parsedHealth))
        {
            AddCriteria(o => o.CropHealth == parsedHealth);
        }
        
        // Filter by pest detection
        if (pestDetected.HasValue)
        {
            AddCriteria(o => o.PestDetected == pestDetected.Value);
        }
        
        // Filter by date range
        if (fromDate.HasValue)
        {
            AddCriteria(o => o.ObservationDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            AddCriteria(o => o.ObservationDate <= toDate.Value);
        }
        
        // Include navigation properties
        AddInclude(o => o.Field);
        AddInclude(o => o.CropCycle);
        AddInclude(o => o.Worker);
        AddInclude(o => o.Farm);
    }
}