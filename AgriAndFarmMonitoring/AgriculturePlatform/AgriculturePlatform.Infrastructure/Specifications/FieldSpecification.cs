// AgriculturePlatform.Infrastructure/Specifications/FieldSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class FieldSpecification : BaseSpecification<Field>
{
    public FieldSpecification(
        int farmId, 
        string? searchTerm = null, 
        string? soilType = null, 
        string? status = null, 
        bool includeDeleted = false,
        bool hasCoordinates = false)  // NEW parameter
    {
        // Base filter - always filter by farm
        AddCriteria(f => f.FarmId == farmId);
        
        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(f => !f.IsDeleted);
        }
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            AddCriteria(f => f.FieldName.Contains(searchTerm) || 
                            (f.Location != null && f.Location.Contains(searchTerm)));
        }
        
        // Soil type filter
        if (!string.IsNullOrWhiteSpace(soilType) && 
            Enum.TryParse<SoilTypeEnum>(soilType, true, out var parsedSoilType))
        {
            AddCriteria(f => f.SoilType == parsedSoilType);
        }
        
        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && 
            Enum.TryParse<FieldStatusEnum>(status, true, out var parsedStatus))
        {
            AddCriteria(f => f.Status == parsedStatus);
        }
        
        // Filter by coordinates presence - NEW
        if (hasCoordinates)
        {
            AddCriteria(f => f.Latitude.HasValue && f.Longitude.HasValue);
        }
        
        // Include navigation properties
        AddInclude(f => f.Farm);
        AddInclude(f => f.Admin);
        AddInclude(f => f.CropCycles);
        
        // Default ordering
        ApplyOrderByDescending(f => f.CreatedAt);
    }
}