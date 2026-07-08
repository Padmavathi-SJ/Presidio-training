// AgriculturePlatform.Application/Services/AlertThresholdService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Services;

public class AlertThresholdService : IAlertThresholdService
{
    private readonly IAlertThresholdRepository _thresholdRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public AlertThresholdService(
        IAlertThresholdRepository thresholdRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _thresholdRepository = thresholdRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<AlertThresholdDto>>> GetAllThresholdsAsync(int farmId)
    {
        var thresholds = await _thresholdRepository.GetAllAsync(farmId);
        var dtos = _mapper.Map<IEnumerable<AlertThresholdDto>>(thresholds);
        return ApiResponse<IEnumerable<AlertThresholdDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<AlertThresholdDto>> GetThresholdByIdAsync(int id, int farmId)
    {
        var threshold = await _thresholdRepository.GetByIdAsync(id, farmId);
        if (threshold == null)
        {
            return ApiResponse<AlertThresholdDto>.Fail($"Threshold with ID {id} not found");
        }
        var dto = _mapper.Map<AlertThresholdDto>(threshold);
        return ApiResponse<AlertThresholdDto>.Ok(dto);
    }

    public async Task<ApiResponse<AlertThresholdDto>> CreateThresholdAsync(CreateAlertThresholdDto dto, int farmId, int adminId)
    {
        // Check if threshold already exists for this crop type and growth stage
        var existing = await _thresholdRepository.GetByCropAndStageAsync(
            dto.CropType, dto.GrowthStage, dto.SensorType, farmId);
        
        if (existing != null)
        {
            return ApiResponse<AlertThresholdDto>.Fail($"Threshold already exists for {dto.CropType} - {dto.GrowthStage} - {dto.SensorType}");
        }

        var threshold = new AlertThreshold
        {
            FarmId = farmId,
            AdminId = adminId,
            CropType = dto.CropType,
            GrowthStage = dto.GrowthStage,
            SensorType = dto.SensorType,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            Severity = dto.Severity,
            IsActive = true,
            NotificationEmails = dto.NotificationEmails,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _thresholdRepository.CreateAsync(threshold);
        await _auditLogService.LogCreateAsync(farmId, adminId, "AlertThreshold", created.Id, created, null, null);

        var result = _mapper.Map<AlertThresholdDto>(created);
        return ApiResponse<AlertThresholdDto>.Ok(result, "Threshold created successfully");
    }

    public async Task<ApiResponse<AlertThresholdDto>> UpdateThresholdAsync(int id, UpdateAlertThresholdDto dto, int farmId, int adminId)
    {
        var threshold = await _thresholdRepository.GetByIdAsync(id, farmId);
        if (threshold == null)
        {
            return ApiResponse<AlertThresholdDto>.Fail($"Threshold with ID {id} not found");
        }

        var oldThreshold = _mapper.Map<AlertThreshold>(threshold);

        if (dto.MinValue.HasValue) threshold.MinValue = dto.MinValue.Value;
        if (dto.MaxValue.HasValue) threshold.MaxValue = dto.MaxValue.Value;
        if (!string.IsNullOrWhiteSpace(dto.Severity)) threshold.Severity = dto.Severity;
        if (dto.IsActive.HasValue) threshold.IsActive = dto.IsActive.Value;
        if (!string.IsNullOrWhiteSpace(dto.NotificationEmails)) threshold.NotificationEmails = dto.NotificationEmails;

        threshold.UpdatedAt = DateTime.UtcNow;
        threshold.UpdatedBy = adminId;

        await _thresholdRepository.UpdateAsync(threshold);
        await _auditLogService.LogUpdateAsync(farmId, adminId, "AlertThreshold", threshold.Id, oldThreshold, threshold, null, null);

        var result = _mapper.Map<AlertThresholdDto>(threshold);
        return ApiResponse<AlertThresholdDto>.Ok(result, "Threshold updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteThresholdAsync(int id, int farmId, int adminId)
    {
        var threshold = await _thresholdRepository.GetByIdAsync(id, farmId);
        if (threshold == null)
        {
            return ApiResponse<bool>.Fail($"Threshold with ID {id} not found");
        }

        await _thresholdRepository.DeleteAsync(threshold);
        await _auditLogService.LogDeleteAsync(farmId, adminId, "AlertThreshold", threshold.Id, threshold, null, null);

        return ApiResponse<bool>.Ok(true, "Threshold deleted successfully");
    }
}
