// AgriculturePlatform.Infrastructure/Specifications/CropCycleSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class CropCycleSpecification : BaseSpecification<CropCycle>
{
    public CropCycleSpecification(
        int farmId,
        int? fieldId,
        string? cropType,
        string? growthStage,
        string? status,
        DateTime? expectedHarvestDateFrom,
        DateTime? expectedHarvestDateTo,
        bool? activeOnly,
        bool? overdueOnly,
        bool includeDeleted)
    {
        // Base filter - by farm
        AddCriteria(c => c.Field != null && c.Field.FarmId == farmId);

        // Soft delete filter
        if (!includeDeleted)
        {
            AddCriteria(c => !c.IsDeleted);
        }

        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(c => c.FieldId == fieldId.Value);
        }

        // Filter by crop type
        if (!string.IsNullOrWhiteSpace(cropType) && Enum.TryParse<CropTypeEnum>(cropType, true, out var parsedCropType))
        {
            AddCriteria(c => c.CropType == parsedCropType);
        }

        // Filter by growth stage
        if (!string.IsNullOrWhiteSpace(growthStage) && Enum.TryParse<GrowthStageEnum>(growthStage, true, out var parsedStage))
        {
            AddCriteria(c => c.GrowthStage == parsedStage);
        }

        // FIX: Filter by status - Convert string to TaskStatusEnum
        if (!string.IsNullOrWhiteSpace(status))
        {
            var enumStatus = status.ToUpper() switch
            {
                "ACTIVE" or "IN_PROGRESS" => TaskStatusEnum.IN_PROGRESS,
                "COMPLETED" or "HARVESTED" => TaskStatusEnum.COMPLETED,
                "CANCELLED" or "FAILED" => TaskStatusEnum.CANCELLED,
                _ => TaskStatusEnum.PENDING
            };
            AddCriteria(c => c.Status == enumStatus);
        }

        // Filter by expected harvest date range
        if (expectedHarvestDateFrom.HasValue)
        {
            AddCriteria(c => c.ExpectedHarvestDate >= expectedHarvestDateFrom.Value);
        }
        if (expectedHarvestDateTo.HasValue)
        {
            AddCriteria(c => c.ExpectedHarvestDate <= expectedHarvestDateTo.Value);
        }

        // Active only filter
        if (activeOnly.HasValue && activeOnly.Value)
        {
            AddCriteria(c => c.Status == TaskStatusEnum.IN_PROGRESS);
        }

        // Overdue only filter
        if (overdueOnly.HasValue && overdueOnly.Value)
        {
            AddCriteria(c => c.ExpectedHarvestDate < DateTime.UtcNow && c.Status == TaskStatusEnum.IN_PROGRESS);
        }

        // Include navigation
        AddInclude(c => c.Field);
    }
}