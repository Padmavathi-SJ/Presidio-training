// AgriculturePlatform.Application/Interfaces/IWeatherRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherRepository
{
    // Read operations
    Task<WeatherData?> GetLatestByFieldAsync(int fieldId, int farmId);
    Task<List<WeatherData>> GetHistoryByFieldAsync(int fieldId, int farmId, DateTime? fromDate, DateTime? toDate);
    Task<PagedResult<WeatherData>> GetPagedHistoryAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate, PaginationParams paginationParams);
    Task<List<WeatherData>> GetWeatherForAllFieldsAsync(int farmId);
    Task<WeatherData?> GetByIdAsync(int id, int farmId);
    
    // Write operations (Admin only)
    Task<WeatherData> CreateAsync(WeatherData weatherData);
    Task UpdateAsync(WeatherData weatherData);
    Task DeleteAsync(WeatherData weatherData);
    
    // Utility
    Task<bool> ExistsForFieldAsync(int fieldId, int farmId);
    
    // ✅ ADD STATISTICS METHODS
    Task<int> GetTotalCountAsync(int farmId);
    Task<int> GetFieldsWithDataCountAsync(int farmId);
    Task<double> GetAverageTemperatureAsync(int farmId);
    Task<double> GetAverageHumidityAsync(int farmId);
    Task<double> GetTotalRainfallAsync(int farmId);
   
}