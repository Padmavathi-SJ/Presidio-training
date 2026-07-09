// AgriculturePlatform.Infrastructure/Repositories/WeatherRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _context;

    public WeatherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WeatherData?> GetLatestByFieldAsync(int fieldId, int farmId)
    {
        return await _context.WeatherData
            .Include(w => w.Field)
            .Where(w => w.FieldId == fieldId && w.FarmId == farmId)
            .OrderByDescending(w => w.RecordedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WeatherData>> GetHistoryByFieldAsync(int fieldId, int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.WeatherData
            .Where(w => w.FieldId == fieldId && w.FarmId == farmId);

        if (fromDate.HasValue)
            query = query.Where(w => w.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(w => w.RecordedAt <= toDate.Value);

        return await query
            .OrderByDescending(w => w.RecordedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<WeatherData>> GetPagedHistoryAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate, PaginationParams paginationParams, List<int>? allowedFieldIds = null)
    {
        var query = _context.WeatherData
            .Include(w => w.Field)
            .Where(w => w.FarmId == farmId);

        if (allowedFieldIds != null && allowedFieldIds.Any())
            query = query.Where(w => allowedFieldIds.Contains(w.FieldId));

        if (fieldId.HasValue)
            query = query.Where(w => w.FieldId == fieldId.Value);
        if (fromDate.HasValue)
            query = query.Where(w => w.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(w => w.RecordedAt <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.RecordedAt)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<WeatherData>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<List<WeatherData>> GetWeatherForAllFieldsAsync(int farmId)
    {
        return await _context.WeatherData
            .Include(w => w.Field)
            .Where(w => w.FarmId == farmId)
            .OrderByDescending(w => w.RecordedAt)
            .ToListAsync();
    }

    public async Task<WeatherData?> GetByIdAsync(int id, int farmId)
    {
        return await _context.WeatherData
            .FirstOrDefaultAsync(w => w.Id == id && w.FarmId == farmId);
    }



// In WeatherRepository.cs
public async Task<WeatherData> CreateAsync(WeatherData weatherData)
{
    if (weatherData.AdminId == 0)
    {
        // Try to get a default admin for the farm
        var defaultAdmin = await _context.Admins
            .Where(a => a.FarmId == weatherData.FarmId && a.IsActive && !a.IsDeleted)
            .FirstOrDefaultAsync();
        
        if (defaultAdmin == null)
            throw new InvalidOperationException("No active admin found for this farm");
        
        weatherData.AdminId = defaultAdmin.Id;
    }
    
    weatherData.RecordedAt = DateTime.UtcNow;
    weatherData.CreatedAt = DateTime.UtcNow;
    await _context.WeatherData.AddAsync(weatherData);
    await _context.SaveChangesAsync();
    return weatherData;
}

    public async Task UpdateAsync(WeatherData weatherData)
    {
        weatherData.UpdatedAt = DateTime.UtcNow;
        _context.WeatherData.Update(weatherData);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(WeatherData weatherData)
    {
        _context.WeatherData.Remove(weatherData);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForFieldAsync(int fieldId, int farmId)
    {
        return await _context.WeatherData
            .AnyAsync(w => w.FieldId == fieldId && w.FarmId == farmId);
    }

    // AgriculturePlatform.Infrastructure/Repositories/WeatherRepository.cs
// Add these methods:

public async Task<int> GetTotalCountAsync(int farmId)
{
    return await _context.WeatherData
        .Where(w => w.FarmId == farmId)
        .CountAsync();
}

public async Task<int> GetFieldsWithDataCountAsync(int farmId)
{
    return await _context.WeatherData
        .Where(w => w.FarmId == farmId)
        .Select(w => w.FieldId)
        .Distinct()
        .CountAsync();
}

public async Task<double> GetAverageTemperatureAsync(int farmId)
{
    var result = await _context.WeatherData
        .Where(w => w.FarmId == farmId && w.Temperature.HasValue)
        .AverageAsync(w => w.Temperature ?? 0);
    
    return Math.Round(result, 1);
}

public async Task<double> GetAverageHumidityAsync(int farmId)
{
    var result = await _context.WeatherData
        .Where(w => w.FarmId == farmId && w.Humidity.HasValue)
        .AverageAsync(w => w.Humidity ?? 0);
    
    return Math.Round(result, 1);
}

public async Task<double> GetTotalRainfallAsync(int farmId)
{
    return await _context.WeatherData
        .Where(w => w.FarmId == farmId && w.RainfallMm.HasValue)
        .SumAsync(w => w.RainfallMm ?? 0);
}

}