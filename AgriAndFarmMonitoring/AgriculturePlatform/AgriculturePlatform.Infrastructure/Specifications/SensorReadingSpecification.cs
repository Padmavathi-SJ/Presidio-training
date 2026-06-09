// AgriculturePlatform.Infrastructure/Specifications/SensorReadingSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class SensorReadingSpecification : BaseSpecification<SensorReading>
{
    public SensorReadingSpecification(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        string? sensorType,
        DateTime? fromDate,
        DateTime? toDate,
        bool? latestOnly)
    {
        // Base filter - by farm
        AddCriteria(s => s.FarmId == farmId);

        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(s => s.FieldId == fieldId.Value);
        }

        // Filter by crop cycle
        if (cropCycleId.HasValue)
        {
            AddCriteria(s => s.CropCycleId == cropCycleId.Value);
        }

        // Filter by sensor type
        if (!string.IsNullOrWhiteSpace(sensorType) && 
            Enum.TryParse<SensorTypeEnum>(sensorType, true, out var parsedSensorType))
        {
            AddCriteria(s => s.SensorType == parsedSensorType);
        }

        // Filter by date range
        if (fromDate.HasValue)
        {
            AddCriteria(s => s.RecordedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            AddCriteria(s => s.RecordedAt <= toDate.Value);
        }

        // Latest only - gets most recent reading per field and sensor type
        if (latestOnly.HasValue && latestOnly.Value)
        {
            // This will be handled in the repository with grouping
            // Add criteria to include only records that are the latest per field/sensor
        }

        // Include navigation properties
        AddInclude(s => s.Field);
        AddInclude(s => s.CropCycle);
        AddInclude(s => s.Alerts);
        
        // Default ordering - newest first
        ApplyOrderByDescending(s => s.RecordedAt);
    }

    // Constructor for latest readings per field
    public static SensorReadingSpecification LatestPerField(int farmId)
    {
        return new SensorReadingSpecification(farmId, null, null, null, null, null, true);
    }

    // Constructor for threshold violations
    public static SensorReadingSpecification ThresholdViolations(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var spec = new SensorReadingSpecification(farmId, null, null, null, fromDate, toDate, false);
        // Add criteria for readings that have associated alerts
        spec.AddCriteria(s => s.Alerts != null && s.Alerts.Any());
        return spec;
    }
}