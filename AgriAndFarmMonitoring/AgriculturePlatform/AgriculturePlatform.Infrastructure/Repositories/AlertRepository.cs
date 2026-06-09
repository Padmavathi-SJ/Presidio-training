// AgriculturePlatform.Infrastructure/Repositories/AlertRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Alert?> GetByIdAsync(int id, int farmId)
    {
        return await _context.Alerts
            .Include(a => a.Field)
            .Include(a => a.CropCycle)
            .FirstOrDefaultAsync(a => a.Id == id && a.FarmId == farmId);
    }

    public async Task<Alert> CreateAsync(Alert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        await _context.Alerts.AddAsync(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task UpdateAsync(Alert alert)
    {
        alert.UpdatedAt = DateTime.UtcNow;
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Alert>> GetPagedAsync(
        int farmId, int? fieldId, int? cropCycleId, string? alertType,
        string? severity, bool? isResolved, DateTime? fromDate, DateTime? toDate,
        PaginationParams paginationParams)
    {
        var specification = new AlertSpecification(
            farmId, fieldId, cropCycleId, alertType, severity, isResolved, fromDate, toDate);

        var query = _context.Alerts
            .Include(a => a.Field)
            .Include(a => a.CropCycle)
            .Where(specification.Criteria!);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            query = paginationParams.IsDescending
                ? query.OrderByDescending(a => EF.Property<object>(a, paginationParams.SortBy))
                : query.OrderBy(a => EF.Property<object>(a, paginationParams.SortBy));
        }
        else
        {
            query = query.OrderByDescending(a => a.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Alert>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<int> GetUnresolvedCountAsync(int farmId)
    {
        return await _context.Alerts
            .CountAsync(a => a.FarmId == farmId && !a.IsResolved);
    }

    public async Task<AlertStatisticsDto> GetStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Alerts.Where(a => a.FarmId == farmId);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value);

        var alerts = await query.ToListAsync();

        var stats = new AlertStatisticsDto
        {
            TotalAlerts = alerts.Count,
            ResolvedAlerts = alerts.Count(a => a.IsResolved),
            UnresolvedAlerts = alerts.Count(a => !a.IsResolved),
            AlertsByType = alerts.GroupBy(a => a.AlertType.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            AlertsBySeverity = alerts.GroupBy(a => a.Severity.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            AlertsByField = alerts.Where(a => a.Field != null)
                .GroupBy(a => a.Field!.FieldName)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentTrend = alerts
                .GroupBy(a => a.CreatedAt.Date)
                .OrderByDescending(g => g.Key)
                .Take(30)
                .Select(g => new AlertTrendDto { Date = g.Key, Count = g.Count() })
                .OrderBy(t => t.Date)
                .ToList()
        };

        return stats;
    }

    public async Task<int> BulkResolveAsync(IEnumerable<int> alertIds, int resolvedBy)
    {
        var alerts = await _context.Alerts
            .Where(a => alertIds.Contains(a.Id) && !a.IsResolved)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.UpdatedBy = resolvedBy;
            alert.UpdatedAt = DateTime.UtcNow;
        }

        _context.Alerts.UpdateRange(alerts);
        return await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Alert>> GetCriticalAlertsAsync(int farmId)
    {
        return await _context.Alerts
            .Include(a => a.Field)
            .Where(a => a.FarmId == farmId && 
                        a.Severity == AlertSeverityEnum.CRITICAL && 
                        !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
}