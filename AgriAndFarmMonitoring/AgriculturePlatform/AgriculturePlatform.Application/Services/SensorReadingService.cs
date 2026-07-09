// AgriculturePlatform.Application/Services/SensorReadingService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class SensorReadingService : ISensorReadingService
{
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IAlertService _alertService;
    private readonly IAlertNotificationService _notificationService;
    private readonly IMapper _mapper;

    public SensorReadingService(
        ISensorReadingRepository sensorRepository,
        IFieldRepository fieldRepository,
        IAlertService alertService,
        IAlertNotificationService notificationService,
        IMapper mapper)
    {
        _sensorRepository = sensorRepository;
        _fieldRepository = fieldRepository;
        _alertService = alertService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<SensorReadingDto>>> GetAllReadingsAsync(
        SensorReadingFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 50,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        // ✅ Parse enum at service level and pass as SensorTypeEnum?
        SensorTypeEnum? parsedSensorType = null;
        if (!string.IsNullOrWhiteSpace(filter.SensorType))
        {
            if (Enum.TryParse<SensorTypeEnum>(filter.SensorType, true, out var parsed))
            {
                parsedSensorType = parsed;
            }
        }

        var pagedResult = await _sensorRepository.GetPagedAsync(
            farmId, 
            filter.FieldId, 
            filter.CropCycleId, 
            parsedSensorType,  // ✅ Pass the enum, not string
            filter.FromDate, 
            filter.ToDate, 
            paginationParams,
            filter.AllowedFieldIds);

        var dtos = _mapper.Map<List<SensorReadingDto>>(pagedResult.Items);
        
        var result = new PagedResult<SensorReadingDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<SensorReadingDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetLatestReadingsPerFieldAsync(int farmId, List<int>? allowedFieldIds = null)
    {
        var readings = await _sensorRepository.GetLatestPerFieldAsync(farmId, allowedFieldIds);
        var dtos = _mapper.Map<IEnumerable<SensorReadingDto>>(readings);
        return ApiResponse<IEnumerable<SensorReadingDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetReadingsByDateRangeAsync(
        int fieldId, int farmId, DateTime fromDate, DateTime toDate)
    {
        // Ensure dates are UTC
        if (fromDate.Kind != DateTimeKind.Utc)
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
        if (toDate.Kind != DateTimeKind.Utc)
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
        
        var readings = await _sensorRepository.GetByFieldAndDateRangeAsync(fieldId, farmId, fromDate, toDate);
        var dtos = _mapper.Map<IEnumerable<SensorReadingDto>>(readings);
        return ApiResponse<IEnumerable<SensorReadingDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetThresholdViolationsAsync(
        int farmId, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null)
    {
        var violations = await _sensorRepository.GetThresholdViolationsAsync(farmId, fromDate, toDate, allowedFieldIds);
        var dtos = _mapper.Map<IEnumerable<SensorReadingDto>>(violations);
        return ApiResponse<IEnumerable<SensorReadingDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<byte[]>> ExportToExcelAsync(
        int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null)
    {
        var fileBytes = await _sensorRepository.ExportToExcelAsync(farmId, fieldId, fromDate, toDate, allowedFieldIds);
        return ApiResponse<byte[]>.Ok(fileBytes);
    }

    public async Task<ApiResponse<SensorStatisticsDto>> GetAverageReadingsAsync(
        int farmId, string groupBy, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null)
    {
        var stats = await _sensorRepository.GetAverageReadingsAsync(farmId, groupBy, fromDate, toDate, allowedFieldIds);
        return ApiResponse<SensorStatisticsDto>.Ok(stats);
    }

    public async Task<ApiResponse<SensorReadingDto>> AddManualReadingAsync(
        CreateManualSensorReadingDto dto, int farmId, int adminId)
    {
        if (!Enum.TryParse<SensorTypeEnum>(dto.SensorType, true, out var sensorType))
        {
            return ApiResponse<SensorReadingDto>.Fail("Invalid sensor type");
        }

        var reading = new SensorReading
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = dto.FieldId,
            CropCycleId = dto.CropCycleId,
            SensorType = sensorType,
            Value = dto.Value,
            Unit = dto.Unit,
            RecordedAt = dto.RecordedAt ?? DateTime.UtcNow
        };

        var created = await _sensorRepository.CreateAsync(reading);

        // Check for alerts immediately
        await _alertService.CheckAndCreateAlertAsync(
            dto.FieldId, dto.CropCycleId, sensorType.ToString(), dto.Value, farmId, adminId);

        var result = _mapper.Map<SensorReadingDto>(created);

        // Notify UI in real-time
        await _notificationService.NotifySensorReadingAsync(farmId, result);

        return ApiResponse<SensorReadingDto>.Ok(result, "Manual reading added successfully");
    }
}