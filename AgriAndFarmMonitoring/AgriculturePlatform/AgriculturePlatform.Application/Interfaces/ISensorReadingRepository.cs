// AgriculturePlatform.Application/Interfaces/ISensorReadingRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Interfaces;

public interface ISensorReadingRepository
{
    Task<SensorReading?> GetByIdAsync(int id, int farmId);
    Task<SensorReading> CreateAsync(SensorReading reading);
    
    Task<PagedResult<SensorReading>> GetPagedAsync(
        int farmId, 
        int? fieldId, 
        int? cropCycleId, 
        SensorTypeEnum? sensorType,  // ✅ Changed from string to SensorTypeEnum?
        DateTime? fromDate, 
        DateTime? toDate, 
        PaginationParams paginationParams,
        List<int>? allowedFieldIds = null);
    
    Task<IEnumerable<SensorReading>> GetLatestPerFieldAsync(int farmId, List<int>? allowedFieldIds = null);
    Task<IEnumerable<SensorReading>> GetByFieldAndDateRangeAsync(int fieldId, int farmId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<SensorReading>> GetThresholdViolationsAsync(int farmId, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null);
    
    Task<byte[]> ExportToExcelAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null);
    
    Task<SensorStatisticsDto> GetAverageReadingsAsync(int farmId, string groupBy, DateTime? fromDate, DateTime? toDate, List<int>? allowedFieldIds = null);
    
    Task<int> BulkCreateAsync(IEnumerable<SensorReading> readings);
}