// AgriculturePlatform.Infrastructure/Specifications/FieldSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class FieldSpecification : BaseSpecification<Field>
{
    public FieldSpecification(
        int farmId, 
        string? searchTerm = null, 
        string? location = null, 
        string? soilType = null, 
        string? status = null, 
        bool includeDeleted = false,
        bool hasCoordinates = false)
    {
        // ✅ Now this will combine with other criteria instead of replacing
        AddCriteria(f => f.FarmId == farmId);
        
        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(f => !f.IsDeleted);
        }
        
     if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.Trim().ToLower();
            AddCriteria(f => f.FieldName.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationLower = location.Trim().ToLower();
            AddCriteria(f => f.Location != null && f.Location.ToLower().Contains(locationLower));
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
        
        // Filter by coordinates presence
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