// AgriculturePlatform.Application/Services/AlertService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IAlertThresholdRepository _thresholdRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IAlertNotificationService _notificationService;  // ✅ Only this

    public AlertService(
        IAlertRepository alertRepository,
        IAlertThresholdRepository thresholdRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IAuditLogService auditLogService,
        IMapper mapper,
        IAlertNotificationService notificationService)  // ✅ Remove hubContext
    {
        _alertRepository = alertRepository;
        _thresholdRepository = thresholdRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PagedResult<AlertDto>>> GetAllAlertsAsync(AlertFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _alertRepository.GetPagedAsync(
            farmId, filter.FieldId, filter.CropCycleId, filter.AlertType,
            filter.Severity, filter.IsResolved, filter.FromDate, filter.ToDate, paginationParams);

        var dtos = _mapper.Map<List<AlertDto>>(pagedResult.Items);
        
        var result = new PagedResult<AlertDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<AlertDto>>.Ok(result);
    }

    public async Task<ApiResponse<AlertDto>> ResolveAlertAsync(int id, ResolveAlertDto dto, int farmId, int adminId)
    {
        var alert = await _alertRepository.GetByIdAsync(id, farmId);
        if (alert == null)
        {
            return ApiResponse<AlertDto>.Fail($"Alert with ID {id} not found");
        }

        if (alert.IsResolved)
        {
            return ApiResponse<AlertDto>.Fail("Alert is already resolved");
        }

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.UpdatedBy = adminId;
        alert.UpdatedAt = DateTime.UtcNow;

        await _alertRepository.UpdateAsync(alert);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Alert", alert.Id, null, 
            new { IsResolved = true, ResolutionNotes = dto.ResolutionNotes }, null, null);

        // ✅ Use notification service instead of direct hub
        await _notificationService.NotifyAlertResolvedAsync(farmId, new { AlertId = id, ResolvedAt = DateTime.UtcNow });

        var result = _mapper.Map<AlertDto>(alert);
        return ApiResponse<AlertDto>.Ok(result, "Alert resolved successfully");
    }

    public async Task<ApiResponse<AlertStatisticsDto>> GetStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _alertRepository.GetStatisticsAsync(farmId, fromDate, toDate);
        return ApiResponse<AlertStatisticsDto>.Ok(stats);
    }

    public async Task<ApiResponse<AlertDto>> CheckAndCreateAlertAsync(
        int fieldId, int cropCycleId, string sensorType, decimal value, int farmId, int adminId)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
        if (field == null) return ApiResponse<AlertDto>.Fail("Field not found");

        var cropCycle = await _cropCycleRepository.GetByIdAsync(cropCycleId, farmId);
        if (cropCycle == null) return ApiResponse<AlertDto>.Fail("Crop cycle not found");

        // Get threshold for this crop type and growth stage
        var threshold = await _thresholdRepository.GetByCropAndStageAsync(
            cropCycle.CropType?.ToString() ?? "", 
            cropCycle.GrowthStage?.ToString() ?? "", 
            sensorType, farmId);

        if (threshold == null || !threshold.IsActive) 
            return ApiResponse<AlertDto>.Ok(null, "No threshold configured");

        // Check if value violates threshold
        bool isViolation = value < threshold.MinValue || value > threshold.MaxValue;
        
        if (!isViolation) return ApiResponse<AlertDto>.Ok(null, "No violation detected");

        // Determine alert type based on sensor
        var alertType = GetAlertType(sensorType, value, threshold);
        var severity = threshold.Severity;

        var alert = new Alert
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = fieldId,
            CropCycleId = cropCycleId,
            AlertType = alertType,
            Severity = Enum.Parse<AlertSeverityEnum>(severity),
            Message = GenerateAlertMessage(alertType, sensorType, value, threshold),
            SensorValue = value,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminId
        };

        var created = await _alertRepository.CreateAsync(alert);

        // ✅ Use notification service instead of direct hub
        await _notificationService.NotifyNewAlertAsync(farmId, new
        {
            Id = created.Id,
            FieldId = fieldId,
            FieldName = field.FieldName,
            AlertType = alertType.ToString(),
            Severity = severity,
            Message = alert.Message,
            CreatedAt = created.CreatedAt
        });

        // Send email notification for critical alerts
        if (severity == "CRITICAL" && !string.IsNullOrEmpty(threshold.NotificationEmails))
        {
            await SendEmailNotificationAsync(threshold.NotificationEmails, alert, field);
        }

        var result = _mapper.Map<AlertDto>(created);
        return ApiResponse<AlertDto>.Ok(result, "Alert created");
    }

    public async Task<ApiResponse<int>> GetUnresolvedCountAsync(int farmId)
    {
        var count = await _alertRepository.GetUnresolvedCountAsync(farmId);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse<IEnumerable<AlertDto>>> GetCriticalAlertsAsync(int farmId)
    {
        var alerts = await _alertRepository.GetCriticalAlertsAsync(farmId);
        var dtos = _mapper.Map<IEnumerable<AlertDto>>(alerts);
        return ApiResponse<IEnumerable<AlertDto>>.Ok(dtos);
    }

    private AlertTypeEnum GetAlertType(string sensorType, decimal value, AlertThreshold threshold)
    {
        return sensorType switch
        {
            "SOIL_MOISTURE" when value < threshold.MinValue => AlertTypeEnum.DROUGHT_STRESS,
            "SOIL_MOISTURE" when value > threshold.MaxValue => AlertTypeEnum.WATERLOGGED,
            "AIR_TEMP" when value > threshold.MaxValue => AlertTypeEnum.HEAT_STRESS,
            "AIR_TEMP" when value < threshold.MinValue => AlertTypeEnum.COLD_STRESS,
            "SOIL_PH" when value < threshold.MinValue || value > threshold.MaxValue => AlertTypeEnum.SOIL_PH_ALERT,
            _ => AlertTypeEnum.IRRIGATION_NEEDED
        };
    }

    private string GenerateAlertMessage(AlertTypeEnum alertType, string sensorType, decimal value, AlertThreshold threshold)
    {
        return alertType switch
        {
            AlertTypeEnum.DROUGHT_STRESS => $"Soil moisture critically low: {value}% (threshold: {threshold.MinValue}%)",
            AlertTypeEnum.WATERLOGGED => $"Excess soil moisture detected: {value}% (threshold: {threshold.MaxValue}%)",
            AlertTypeEnum.HEAT_STRESS => $"High temperature stress: {value}°C (threshold: {threshold.MaxValue}°C)",
            AlertTypeEnum.COLD_STRESS => $"Low temperature stress: {value}°C (threshold: {threshold.MinValue}°C)",
            AlertTypeEnum.SOIL_PH_ALERT => $"Soil pH out of range: {value} (optimal: {threshold.MinValue}-{threshold.MaxValue})",
            _ => $"{sensorType} reading {value} is outside optimal range (optimal: {threshold.MinValue}-{threshold.MaxValue})"
        };
    }

    private async Task SendEmailNotificationAsync(string emails, Alert alert, Field field)
    {
        // TODO: Implement email sending logic here
        // This could use SendGrid, SMTP, or other email services
        await Task.CompletedTask;
    }
}