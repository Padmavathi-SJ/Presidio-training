// AgriculturePlatform.Application/Interfaces/ISensorReadingService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;

namespace AgriculturePlatform.Application.Interfaces;

public interface ISensorReadingService
{
    Task<ApiResponse<PagedResult<SensorReadingDto>>> GetAllReadingsAsync(SensorReadingFilterDto filter, int farmId);
    Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetLatestReadingsPerFieldAsync(int farmId);
    Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetReadingsByDateRangeAsync(int fieldId, int farmId, DateTime fromDate, DateTime toDate);
    Task<ApiResponse<IEnumerable<SensorReadingDto>>> GetThresholdViolationsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<byte[]>> ExportToExcelAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<SensorStatisticsDto>> GetAverageReadingsAsync(int farmId, string groupBy, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<SensorReadingDto>> AddManualReadingAsync(CreateManualSensorReadingDto dto, int farmId, int adminId);
}