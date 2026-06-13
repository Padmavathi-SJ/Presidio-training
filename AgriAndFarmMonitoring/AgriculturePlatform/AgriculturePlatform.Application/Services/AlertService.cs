// Application/Services/AlertService.cs
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
    private readonly IAlertThresholdRepository _alertThresholdRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IAlertNotificationService _notificationService;

    public AlertService(
        IAlertRepository alertRepository,
        IAlertThresholdRepository alertThresholdRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IAuditLogService auditLogService,
        IMapper mapper,
        IAlertNotificationService notificationService)
    {
        _alertRepository = alertRepository;
        _alertThresholdRepository = alertThresholdRepository;
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

    public async Task<ApiResponse<AlertDto>> GetAlertByIdAsync(int id, int farmId)
    {
        var alert = await _alertRepository.GetByIdAsync(id, farmId);
        if (alert == null)
        {
            return ApiResponse<AlertDto>.Fail($"Alert with ID {id} not found");
        }

        var result = _mapper.Map<AlertDto>(alert);
        return ApiResponse<AlertDto>.Ok(result);
    }

    public async Task<ApiResponse<AlertStatisticsDto>> GetAlertStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _alertRepository.GetStatisticsAsync(farmId, fromDate, toDate);
        return ApiResponse<AlertStatisticsDto>.Ok(stats);
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

        await _notificationService.NotifyAlertResolvedAsync(farmId, new { AlertId = id, ResolvedAt = DateTime.UtcNow });

        var result = _mapper.Map<AlertDto>(alert);
        return ApiResponse<AlertDto>.Ok(result, "Alert resolved successfully");
    }

    public async Task<Alert?> CheckAndCreateAlertAsync(int fieldId, int cropCycleId, string sensorType, decimal value, int farmId, int adminId)
    {
        // Get thresholds for this sensor type, crop type, and growth stage
        var cropCycle = await _cropCycleRepository.GetByIdAsync(cropCycleId, farmId);
        if (cropCycle == null) return null;
        
        var threshold = await _alertThresholdRepository.GetThresholdsAsync(
            farmId, cropCycle.CropType?.ToString(), cropCycle.GrowthStage?.ToString(), sensorType);
        
        if (threshold == null) return null;
        
        AlertSeverityEnum? severity = null;
        string message = null;
        
        // Check if value exceeds threshold
        if (value < threshold.MinValue)
        {
            severity = GetSeverityForDeviation(threshold.MinValue - value, threshold.MinValue);
            message = $"{sensorType} is too LOW: {value} (Min: {threshold.MinValue})";
        }
        else if (value > threshold.MaxValue)
        {
            severity = GetSeverityForDeviation(value - threshold.MaxValue, threshold.MaxValue);
            message = $"{sensorType} is too HIGH: {value} (Max: {threshold.MaxValue})";
        }
        
        if (severity != null)
        {
            var alert = new Alert
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = fieldId,
                CropCycleId = cropCycleId,
                AlertType = GetAlertType(sensorType, value, threshold),  // ✅ Pass parameters
                Severity = severity,
                Message = message,
                SensorValue = value,
                ThresholdValue = severity == AlertSeverityEnum.CRITICAL ? threshold.MaxValue : threshold.MinValue,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };
            
            var createdAlert = await _alertRepository.CreateAsync(alert);
            
            // Send email notifications for HIGH and CRITICAL alerts
            if (severity == AlertSeverityEnum.HIGH || severity == AlertSeverityEnum.CRITICAL)
            {
                await _notificationService.SendAlertNotificationsAsync(createdAlert, farmId);
            }
            
            return createdAlert;
        }
        
        return null;
    }

    private AlertSeverityEnum GetSeverityForDeviation(decimal deviation, decimal threshold)
    {
        var deviationPercentage = (deviation / threshold) * 100;
        
        if (deviationPercentage >= 30)
            return AlertSeverityEnum.CRITICAL;
        else if (deviationPercentage >= 15)
            return AlertSeverityEnum.HIGH;
        else if (deviationPercentage >= 5)
            return AlertSeverityEnum.MEDIUM;
        else
            return AlertSeverityEnum.LOW;
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
        await Task.CompletedTask;
    }
}