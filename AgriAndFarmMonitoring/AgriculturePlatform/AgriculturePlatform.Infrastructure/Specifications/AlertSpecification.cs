// AgriculturePlatform.Infrastructure/Specifications/AlertSpecification.cs
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Specifications;

public class AlertSpecification : BaseSpecification<Alert>
{
    public AlertSpecification(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        string? alertType,
        string? severity,
        bool? isResolved,
        DateTime? fromDate,
        DateTime? toDate)
    {
        // Base filter - by farm
        AddCriteria(a => a.FarmId == farmId);

        // Filter by field
        if (fieldId.HasValue)
        {
            AddCriteria(a => a.FieldId == fieldId.Value);
        }

        // Filter by crop cycle
        if (cropCycleId.HasValue)
        {
            AddCriteria(a => a.CropCycleId == cropCycleId.Value);
        }

        // Filter by alert type
        if (!string.IsNullOrWhiteSpace(alertType) && 
            Enum.TryParse<AlertTypeEnum>(alertType, true, out var parsedAlertType))
        {
            AddCriteria(a => a.AlertType == parsedAlertType);
        }

        // Filter by severity
        if (!string.IsNullOrWhiteSpace(severity) && 
            Enum.TryParse<AlertSeverityEnum>(severity, true, out var parsedSeverity))
        {
            AddCriteria(a => a.Severity == parsedSeverity);
        }

        // Filter by resolved status
        if (isResolved.HasValue)
        {
            AddCriteria(a => a.IsResolved == isResolved.Value);
        }

        // Filter by date range
        if (fromDate.HasValue)
        {
            AddCriteria(a => a.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            AddCriteria(a => a.CreatedAt <= toDate.Value);
        }

        // Include navigation properties
        AddInclude(a => a.Field);
        AddInclude(a => a.CropCycle);
    }
}