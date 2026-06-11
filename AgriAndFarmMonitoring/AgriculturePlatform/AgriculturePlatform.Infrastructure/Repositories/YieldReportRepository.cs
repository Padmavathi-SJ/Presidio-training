// Infrastructure/Repositories/YieldReportRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class YieldReportRepository : IYieldReportRepository
{
    private readonly AppDbContext _context;

    public YieldReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<YieldReport?> GetByIdAsync(int id, int farmId)
    {
        return await _context.YieldReports
            .Include(r => r.Farm)
            .Include(r => r.CropCycle)
            .Include(r => r.Field)
            .Include(r => r.Admin)
            .FirstOrDefaultAsync(r => r.Id == id && r.FarmId == farmId && !r.IsDeleted);
    }

    public async Task<YieldReport> CreateAsync(YieldReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        await _context.YieldReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(YieldReport report)
    {
        report.UpdatedAt = DateTime.UtcNow;
        _context.YieldReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(YieldReport report)
    {
        report.IsDeleted = true;
        report.DeletedAt = DateTime.UtcNow;
        _context.YieldReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.YieldReports
            .AnyAsync(r => r.Id == id && r.FarmId == farmId && !r.IsDeleted);
    }

    public async Task<PagedResult<YieldReport>> GetPagedAsync(
        int farmId,
        int? cropCycleId,
        int? fieldId,
        string? reportType,
        DateTime? fromDate,
        DateTime? toDate,
        bool? isScheduled,
        PaginationParams paginationParams)
    {
        var query = _context.YieldReports
            .Include(r => r.Farm)
            .Include(r => r.CropCycle)
            .Include(r => r.Field)
            .Where(r => r.FarmId == farmId && !r.IsDeleted);

        if (cropCycleId.HasValue)
            query = query.Where(r => r.CropCycleId == cropCycleId.Value);
        
        if (fieldId.HasValue)
            query = query.Where(r => r.FieldId == fieldId.Value);
        
        if (!string.IsNullOrWhiteSpace(reportType))
            query = query.Where(r => r.ReportType == reportType);
        
        if (fromDate.HasValue)
            query = query.Where(r => r.StartDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(r => r.EndDate <= toDate.Value);
        
        if (isScheduled.HasValue)
            query = query.Where(r => r.IsScheduled == isScheduled.Value);

        query = paginationParams.IsDescending
            ? query.OrderByDescending(r => EF.Property<object>(r, paginationParams.SortBy ?? "CreatedAt"))
            : query.OrderBy(r => EF.Property<object>(r, paginationParams.SortBy ?? "CreatedAt"));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<YieldReport>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<YieldReport>> GetByCropCycleAsync(int cropCycleId, int farmId)
    {
        return await _context.YieldReports
            .Include(r => r.Field)
            .Where(r => r.CropCycleId == cropCycleId && r.FarmId == farmId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<YieldReport>> GetByFieldAsync(int fieldId, int farmId)
    {
        return await _context.YieldReports
            .Include(r => r.CropCycle)
            .Where(r => r.FieldId == fieldId && r.FarmId == farmId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<YieldReport>> GetScheduledReportsAsync(int farmId)
    {
        return await _context.YieldReports
            .Where(r => r.FarmId == farmId && r.IsScheduled && !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<YieldReport?> GetLatestReportAsync(int farmId, int? cropCycleId, int? fieldId)
    {
        var query = _context.YieldReports
            .Where(r => r.FarmId == farmId && !r.IsDeleted);

        if (cropCycleId.HasValue)
            query = query.Where(r => r.CropCycleId == cropCycleId.Value);
        if (fieldId.HasValue)
            query = query.Where(r => r.FieldId == fieldId.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetTotalYieldForPeriodAsync(int farmId, DateTime startDate, DateTime endDate, int? fieldId = null)
    {
        var query = _context.Harvests
            .Where(h => h.FarmId == farmId && 
                       h.HarvestDate >= startDate && 
                       h.HarvestDate <= endDate &&
                       h.ApprovalStatus == "APPROVED" &&
                       !h.IsDeleted);

        if (fieldId.HasValue)
            query = query.Where(h => h.FieldId == fieldId.Value);

        return await query.SumAsync(h => h.QuantityKg);
    }
}