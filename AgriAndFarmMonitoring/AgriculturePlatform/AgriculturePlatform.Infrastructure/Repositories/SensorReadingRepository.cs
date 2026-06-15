// AgriculturePlatform.Infrastructure/Repositories/SensorReadingRepository.cs
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;


namespace AgriculturePlatform.Infrastructure.Repositories;

public class SensorReadingRepository : ISensorReadingRepository
{
    private readonly AppDbContext _context;

    public SensorReadingRepository(AppDbContext context)
    {
        _context = context;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<SensorReading?> GetByIdAsync(int id, int farmId)
    {
        return await _context.SensorReadings
            .Include(s => s.Field)
            .Include(s => s.CropCycle)
            .FirstOrDefaultAsync(s => s.Id == id && s.FarmId == farmId);
    }

    public async Task<SensorReading> CreateAsync(SensorReading reading)
    {
        reading.RecordedAt = DateTime.UtcNow;
        await _context.SensorReadings.AddAsync(reading);
        await _context.SaveChangesAsync();
        return reading;
    }

    public async Task<PagedResult<SensorReading>> GetPagedAsync(
        int farmId, int? fieldId, int? cropCycleId, string? sensorType,
        DateTime? fromDate, DateTime? toDate, PaginationParams paginationParams)
    {
        var query = _context.SensorReadings
            .Include(s => s.Field)
            .Include(s => s.CropCycle)
            .Where(s => s.FarmId == farmId);

        if (fieldId.HasValue)
            query = query.Where(s => s.FieldId == fieldId.Value);
        if (cropCycleId.HasValue)
            query = query.Where(s => s.CropCycleId == cropCycleId.Value);
        if (!string.IsNullOrWhiteSpace(sensorType))
            query = query.Where(s => s.SensorType.ToString() == sensorType);
        if (fromDate.HasValue)
            query = query.Where(s => s.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.RecordedAt <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.RecordedAt)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<SensorReading>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<SensorReading>> GetLatestPerFieldAsync(int farmId)
    {
        return await _context.SensorReadings
            .Include(s => s.Field)
            .Where(s => s.FarmId == farmId)
            .GroupBy(s => new { s.FieldId, s.SensorType })
            .Select(g => g.OrderByDescending(s => s.RecordedAt).FirstOrDefault())
            .ToListAsync();
    }

// AgriculturePlatform.Infrastructure/Repositories/SensorReadingRepository.cs

public async Task<IEnumerable<SensorReading>> GetByFieldAndDateRangeAsync(
    int fieldId, int farmId, DateTime fromDate, DateTime toDate)
{
    // Convert to UTC and use date part only
    var fromDateUtc = fromDate.ToUniversalTime().Date;
    var toDateUtc = toDate.ToUniversalTime().Date;
    
    return await _context.SensorReadings
        .Include(s => s.Field)
        .Where(s => s.FieldId == fieldId && s.FarmId == farmId && 
                    s.RecordedAt.Date >= fromDateUtc && s.RecordedAt.Date <= toDateUtc)
        .OrderBy(s => s.RecordedAt)
        .ToListAsync();
}

    public async Task<IEnumerable<SensorReading>> GetThresholdViolationsAsync(
        int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.SensorReadings
            .Include(s => s.Field)
            .Include(s => s.Alerts)
            .Where(s => s.FarmId == farmId && s.Alerts.Any());

        if (fromDate.HasValue)
            query = query.Where(s => s.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.RecordedAt <= toDate.Value);

        return await query.ToListAsync();
    }

    public async Task<byte[]> ExportToExcelAsync(int farmId, int? fieldId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.SensorReadings
            .Include(s => s.Field)
            .Where(s => s.FarmId == farmId);

        if (fieldId.HasValue)
            query = query.Where(s => s.FieldId == fieldId.Value);
        if (fromDate.HasValue)
            query = query.Where(s => s.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.RecordedAt <= toDate.Value);

        var readings = await query.ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sensor Readings");

        // Headers
        worksheet.Cells[1, 1].Value = "Field";
        worksheet.Cells[1, 2].Value = "Crop Cycle";
        worksheet.Cells[1, 3].Value = "Sensor Type";
        worksheet.Cells[1, 4].Value = "Value";
        worksheet.Cells[1, 5].Value = "Unit";
        worksheet.Cells[1, 6].Value = "Recorded At";

        for (int i = 0; i < readings.Count; i++)
        {
            var reading = readings[i];
            var row = i + 2;
            worksheet.Cells[row, 1].Value = reading.Field?.FieldName;
            worksheet.Cells[row, 2].Value = reading.CropCycleId;
            worksheet.Cells[row, 3].Value = reading.SensorType.ToString();
            worksheet.Cells[row, 4].Value = reading.Value;
            worksheet.Cells[row, 5].Value = reading.Unit;
            worksheet.Cells[row, 6].Value = reading.RecordedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<SensorStatisticsDto> GetAverageReadingsAsync(
        int farmId, string groupBy, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.SensorReadings.Where(s => s.FarmId == farmId);
        
        if (fromDate.HasValue)
            query = query.Where(s => s.RecordedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.RecordedAt <= toDate.Value);

        var stats = new SensorStatisticsDto { Period = groupBy };
        var readings = await query.ToListAsync();

        if (groupBy?.ToLower() == "day")
        {
            stats.DailyStats = readings
                .GroupBy(s => s.RecordedAt.Date)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => new DailySensorStats
                    {
                        Date = g.Key,
                        AvgSoilMoisture = g.Where(s => s.SensorType == SensorTypeEnum.SOIL_MOISTURE).Average(s => s.Value),
                        AvgSoilTemp = g.Where(s => s.SensorType == SensorTypeEnum.SOIL_TEMP).Average(s => s.Value),
                        AvgAirTemp = g.Where(s => s.SensorType == SensorTypeEnum.AIR_TEMP).Average(s => s.Value),
                        AvgHumidity = g.Where(s => s.SensorType == SensorTypeEnum.AIR_HUMIDITY).Average(s => s.Value),
                        ReadingsCount = g.Count()
                    });
        }
        else if (groupBy?.ToLower() == "week")
        {
            stats.WeeklyStats = readings
                .GroupBy(s => new { Year = s.RecordedAt.Year, Week = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(s.RecordedAt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) })
                .ToDictionary(
                    g => $"{g.Key.Year}-W{g.Key.Week}",
                    g => new WeeklySensorStats
                    {
                        WeekNumber = g.Key.Week,
                        Year = g.Key.Year,
                        AvgSoilMoisture = g.Where(s => s.SensorType == SensorTypeEnum.SOIL_MOISTURE).Average(s => s.Value),
                        AvgSoilTemp = g.Where(s => s.SensorType == SensorTypeEnum.SOIL_TEMP).Average(s => s.Value),
                        AlertCount = g.SelectMany(s => s.Alerts).Count()
                    });
        }

        return stats;
    }

    public async Task<int> BulkCreateAsync(IEnumerable<SensorReading> readings)
    {
        await _context.SensorReadings.AddRangeAsync(readings);
        return await _context.SaveChangesAsync();
    }
}