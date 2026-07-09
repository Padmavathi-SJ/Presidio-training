// AgriculturePlatform.Infrastructure/Repositories/WeatherAlertRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class WeatherAlertRepository : IWeatherAlertRepository
{
    private readonly AppDbContext _context;

    public WeatherAlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WeatherAlert?> GetByIdAsync(int id, int farmId)
    {
        return await _context.WeatherAlerts
            .Include(w => w.Field)
            .Include(w => w.Admin)
            .Include(w => w.Acknowledger)
            .FirstOrDefaultAsync(w => w.Id == id && w.FarmId == farmId);
    }

    public async Task<List<WeatherAlert>> GetActiveAlertsAsync(int farmId, List<int>? allowedFieldIds = null)
    {
        var now = DateTime.UtcNow;
        var query = _context.WeatherAlerts
            .Include(w => w.Field)
            .Where(w => w.FarmId == farmId 
                        && !w.IsAcknowledged 
                        && (w.ExpiresAt == null || w.ExpiresAt > now));
                        
        if (allowedFieldIds != null && allowedFieldIds.Any())
            query = query.Where(w => allowedFieldIds.Contains(w.FieldId));
            
        return await query
            .OrderByDescending(w => w.Severity)
            .ThenByDescending(w => w.AlertTime)
            .ToListAsync();
    }

    public async Task<List<WeatherAlert>> GetAlertsByFieldAsync(int fieldId, int farmId)
    {
        return await _context.WeatherAlerts
            .Include(w => w.Field)
            .Where(w => w.FieldId == fieldId && w.FarmId == farmId)
            .OrderByDescending(w => w.AlertTime)
            .ToListAsync();
    }

    public async Task<List<WeatherAlert>> GetAlertsBySeverityAsync(int farmId, string severity)
    {
        var parsedSeverity = Enum.Parse<WeatherAlertSeverityEnum>(severity, true);
        return await _context.WeatherAlerts
            .Include(w => w.Field)
            .Where(w => w.FarmId == farmId && w.Severity == parsedSeverity)
            .OrderByDescending(w => w.AlertTime)
            .ToListAsync();
    }

    public async Task<PagedResult<WeatherAlert>> GetPagedAlertsAsync(
        int farmId, 
        int? fieldId, 
        string? severity, 
        bool? isAcknowledged, 
        PaginationParams paginationParams,
        List<int>? allowedFieldIds = null)
    {
        var query = _context.WeatherAlerts
            .Include(w => w.Field)
            .Where(w => w.FarmId == farmId);

        if (allowedFieldIds != null && allowedFieldIds.Any())
            query = query.Where(w => allowedFieldIds.Contains(w.FieldId));

        if (fieldId.HasValue)
            query = query.Where(w => w.FieldId == fieldId.Value);
        
        if (!string.IsNullOrWhiteSpace(severity))
        {
            var parsedSeverity = Enum.Parse<WeatherAlertSeverityEnum>(severity, true);
            query = query.Where(w => w.Severity == parsedSeverity);
        }
        
        if (isAcknowledged.HasValue)
            query = query.Where(w => w.IsAcknowledged == isAcknowledged.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.AlertTime)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<WeatherAlert>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<WeatherAlert> CreateAsync(WeatherAlert alert)
    {
        alert.AlertTime = DateTime.UtcNow;
        alert.CreatedAt = DateTime.UtcNow;
        
        await _context.WeatherAlerts.AddAsync(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task UpdateAsync(WeatherAlert alert)
    {
        alert.UpdatedAt = DateTime.UtcNow;
        _context.WeatherAlerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(WeatherAlert alert)
    {
        _context.WeatherAlerts.Remove(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<int> AcknowledgeAlertAsync(int id, int acknowledgedBy, int farmId)
    {
        var alert = await _context.WeatherAlerts
            .FirstOrDefaultAsync(w => w.Id == id && w.FarmId == farmId);

        if (alert == null)
            return 0;

        alert.IsAcknowledged = true;
        alert.AcknowledgedBy = acknowledgedBy;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        _context.WeatherAlerts.Update(alert);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> AcknowledgeAllByFieldAsync(int fieldId, int acknowledgedBy, int farmId)
    {
        var alerts = await _context.WeatherAlerts
            .Where(w => w.FieldId == fieldId 
                        && w.FarmId == farmId 
                        && !w.IsAcknowledged)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.IsAcknowledged = true;
            alert.AcknowledgedBy = acknowledgedBy;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.UpdatedAt = DateTime.UtcNow;
        }

        _context.WeatherAlerts.UpdateRange(alerts);
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.WeatherAlerts
            .AnyAsync(w => w.Id == id && w.FarmId == farmId);
    }

    public async Task<int> GetActiveAlertCountAsync(int farmId)
    {
        var now = DateTime.UtcNow;
        return await _context.WeatherAlerts
            .CountAsync(w => w.FarmId == farmId 
                             && !w.IsAcknowledged 
                             && (w.ExpiresAt == null || w.ExpiresAt > now));
    }

    public async Task<int> GetCriticalAlertCountAsync(int farmId)
{
    var now = DateTime.UtcNow;
    return await _context.WeatherAlerts
        .CountAsync(w => w.FarmId == farmId 
            && !w.IsAcknowledged 
            && (w.ExpiresAt == null || w.ExpiresAt > now)
            && (w.Severity == WeatherAlertSeverityEnum.WARNING 
                || w.Severity == WeatherAlertSeverityEnum.EMERGENCY));
}
}