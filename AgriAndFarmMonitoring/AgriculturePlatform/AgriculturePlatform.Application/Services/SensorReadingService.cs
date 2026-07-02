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
    private readonly IMapper _mapper;

    public SensorReadingService(
        ISensorReadingRepository sensorRepository,
        IFieldRepository fieldRepository,
        IMapper mapper)
    {
        _sensorRepository = sensorRepository;
        _fieldRepository = fieldRepository;
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
            paginationParams);

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

    public async Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetLatestReadingsPerFieldAsync(int farmId)
    {
        var readings = await _sensorRepository.GetLatestPerFieldAsync(farmId);
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
        int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var violations = await _sensorRepository.GetThresholdViolationsAsync(farmId, fromDate, toDate);
        var dtos = _mapper.Map<IEnumerable<SensorReadingDto>>(violations);
        return ApiResponse<IEnumerable<SensorReadingDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<byte[]>> ExportToExcelAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate)
    {
        var excelData = await _sensorRepository.ExportToExcelAsync(farmId, fieldId, fromDate, toDate);
        return ApiResponse<byte[]>.Ok(excelData);
    }

    public async Task<ApiResponse<SensorStatisticsDto>> GetAverageReadingsAsync(
        int farmId, string groupBy, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _sensorRepository.GetAverageReadingsAsync(farmId, groupBy, fromDate, toDate);
        return ApiResponse<SensorStatisticsDto>.Ok(stats);
    }
}