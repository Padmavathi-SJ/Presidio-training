// AgriculturePlatform.Application/Interfaces/ISensorReadingRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface ISensorReadingRepository
{
    Task<SensorReading?> GetByIdAsync(int id, int farmId);
    Task<SensorReading> CreateAsync(SensorReading reading);
    Task<PagedResult<SensorReading>> GetPagedAsync(
        int farmId, int? fieldId, int? cropCycleId, string? sensorType,
        DateTime? fromDate, DateTime? toDate, PaginationParams paginationParams);
    
    Task<IEnumerable<SensorReading>> GetLatestPerFieldAsync(int farmId);
    Task<IEnumerable<SensorReading>> GetByFieldAndDateRangeAsync(int fieldId, int farmId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<SensorReading>> GetThresholdViolationsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    
    Task<byte[]> ExportToExcelAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate);
    
    Task<SensorStatisticsDto> GetAverageReadingsAsync(int farmId, string groupBy, DateTime? fromDate, DateTime? toDate);
    
    Task<int> BulkCreateAsync(IEnumerable<SensorReading> readings);
}