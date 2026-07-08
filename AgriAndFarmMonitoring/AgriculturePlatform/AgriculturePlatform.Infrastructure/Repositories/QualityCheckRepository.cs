// Infrastructure/Repositories/QualityCheckRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class QualityCheckRepository : IQualityCheckRepository
{
    private readonly AppDbContext _context;

    public QualityCheckRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QualityCheck?> GetByIdAsync(int id, int farmId)
    {
        return await _context.QualityChecks
            .Include(q => q.Harvest)
            .Include(q => q.Checker)
            .Include(q => q.Approver)
            .Include(q => q.Farm)
            .FirstOrDefaultAsync(q => q.Id == id && q.FarmId == farmId && !q.IsDeleted);
    }

    public async Task<QualityCheck> CreateAsync(QualityCheck qualityCheck)
    {
        qualityCheck.CreatedAt = DateTime.UtcNow;
        await _context.QualityChecks.AddAsync(qualityCheck);
        await _context.SaveChangesAsync();
        return qualityCheck;
    }

    public async Task UpdateAsync(QualityCheck qualityCheck)
    {
        qualityCheck.UpdatedAt = DateTime.UtcNow;
        _context.QualityChecks.Update(qualityCheck);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(QualityCheck qualityCheck, int deletedBy)
    {
        qualityCheck.IsDeleted = true;
        qualityCheck.DeletedAt = DateTime.UtcNow;
        qualityCheck.DeletedBy = deletedBy;
        qualityCheck.UpdatedAt = DateTime.UtcNow;
        _context.QualityChecks.Update(qualityCheck);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.QualityChecks
            .AnyAsync(q => q.Id == id && q.FarmId == farmId && !q.IsDeleted);
    }

    public async Task<PagedResult<QualityCheck>> GetPagedAsync(
        int farmId,
        int? harvestId,
        int? workerId,
        string? approvalStatus,
        string? finalGrade,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var query = _context.QualityChecks
        .Include(q => q.Farm)           // ✅ Add this
        .Include(q => q.Harvest)        // ✅ Add this
        .Include(q => q.Checker)        // ✅ Add this
        .Include(q => q.Approver)       // ✅ Add this
        .Where(q => q.FarmId == farmId);
        
        if (!includeDeleted)
            query = query.Where(q => !q.IsDeleted);

        if (harvestId.HasValue)
            query = query.Where(q => q.HarvestId == harvestId.Value);
        
        if (workerId.HasValue)
            query = query.Where(q => q.CheckedBy == workerId.Value);
        
        if (!string.IsNullOrWhiteSpace(approvalStatus))
            query = query.Where(q => q.ApprovalStatus == approvalStatus);
        
        if (!string.IsNullOrWhiteSpace(finalGrade) && Enum.TryParse<QualityGradeEnum>(finalGrade, true, out var grade))
            query = query.Where(q => q.FinalGrade == grade);
        
        if (fromDate.HasValue)
            query = query.Where(q => q.CheckDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(q => q.CheckDate <= toDate.Value);

        query = paginationParams.IsDescending
            ? query.OrderByDescending(q => EF.Property<object>(q, paginationParams.SortBy ?? "CheckDate"))
            : query.OrderBy(q => EF.Property<object>(q, paginationParams.SortBy ?? "CheckDate"));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<QualityCheck>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<QualityCheck>> GetByHarvestAsync(int harvestId, int farmId)
    {
        return await _context.QualityChecks
        .Include(q => q.Farm)           // ✅ Add this
        .Include(q => q.Harvest)        // ✅ Add this
        .Include(q => q.Checker)        // ✅ Add this
        .Include(q => q.Approver)       // ✅ Add this
        .Where(q => q.HarvestId == harvestId && q.FarmId == farmId && !q.IsDeleted)
        .OrderByDescending(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<QualityCheck>> GetByWorkerAsync(int workerId, int farmId)
    {
        return await _context.QualityChecks
        .Include(q => q.Farm)           // ✅ Add this
        .Include(q => q.Harvest)        // ✅ Add this
        .Include(q => q.Checker)        // ✅ Add this
        .Include(q => q.Approver)       // ✅ Add this
        .Where(q => q.CheckedBy == workerId && q.FarmId == farmId && !q.IsDeleted)
        .OrderByDescending(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<IEnumerable<QualityCheck>> GetPendingApprovalsAsync(int farmId)
    {
        return await _context.QualityChecks
        .Include(q => q.Farm)           // ✅ Add this
        .Include(q => q.Harvest)        // ✅ Add this
        .Include(q => q.Checker)        // ✅ Add this
        .Include(q => q.Approver)       // ✅ Add this
        .Where(q => q.FarmId == farmId && q.ApprovalStatus == "PENDING" && !q.IsDeleted)
        .OrderBy(q => q.CreatedAt)
        .ToListAsync();
    }

    public async Task<IEnumerable<QualityCheck>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate)
    {
       return await _context.QualityChecks
        .Include(q => q.Farm)           // ✅ Add this
        .Include(q => q.Harvest)        // ✅ Add this
        .Include(q => q.Checker)        // ✅ Add this
        .Include(q => q.Approver)       // ✅ Add this
        .Where(q => q.FarmId == farmId && q.CheckDate >= fromDate && q.CheckDate <= toDate && !q.IsDeleted)
        .OrderBy(q => q.CheckDate)
        .ToListAsync();
    }

    public async Task<QualityStatisticsDto> GetQualityStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate, int? workerId = null)
    {
        var query = _context.QualityChecks
            .Include(q => q.Checker)
            .Include(q => q.Harvest)
            .Where(q => q.FarmId == farmId && !q.IsDeleted);

        if (workerId.HasValue)
            query = query.Where(q => q.CheckedBy == workerId.Value);

        if (fromDate.HasValue)
            query = query.Where(q => q.CheckDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(q => q.CheckDate <= toDate.Value);

        var checks = await query.ToListAsync();

        var stats = new QualityStatisticsDto
        {
            TotalChecks = checks.Count,
            ApprovedChecks = checks.Count(q => q.ApprovalStatus == "APPROVED"),
            RejectedChecks = checks.Count(q => q.ApprovalStatus == "REJECTED"),
            PendingChecks = checks.Count(q => q.ApprovalStatus == "PENDING"),
            GradeDistribution = checks.Where(q => q.FinalGrade.HasValue)
                .GroupBy(q => q.FinalGrade!.Value.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            QualityByWorker = checks.Where(q => q.Checker != null)
                .GroupBy(q => q.Checker!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            QualityByHarvest = checks.Where(q => q.Harvest != null && q.Harvest.BatchNumber != null)
                .GroupBy(q => q.Harvest!.BatchNumber!)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageMoisturePct = checks.Any(q => q.MoisturePct.HasValue) ? checks.Where(q => q.MoisturePct.HasValue).Average(q => q.MoisturePct ?? 0) : 0,
            AverageDefectPct = checks.Any(q => q.DefectPct.HasValue) ? checks.Where(q => q.DefectPct.HasValue).Average(q => q.DefectPct ?? 0) : 0,
            MinMoisturePct = checks.Any(q => q.MoisturePct.HasValue) ? checks.Where(q => q.MoisturePct.HasValue).Min(q => q.MoisturePct ?? 0) : 0,
            MaxMoisturePct = checks.Any(q => q.MoisturePct.HasValue) ? checks.Where(q => q.MoisturePct.HasValue).Max(q => q.MoisturePct ?? 0) : 0,
            MinDefectPct = checks.Any(q => q.DefectPct.HasValue) ? checks.Where(q => q.DefectPct.HasValue).Min(q => q.DefectPct ?? 0) : 0,
            MaxDefectPct = checks.Any(q => q.DefectPct.HasValue) ? checks.Where(q => q.DefectPct.HasValue).Max(q => q.DefectPct ?? 0) : 0,
            MonthlyTrend = checks
                .GroupBy(q => new { q.CheckDate.Year, q.CheckDate.Month })
                .Select(g => new MonthlyQualityTrendDto
                {
                    Year = g.Key.Year,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    TotalChecks = g.Count(),
                    PassCount = g.Count(q => q.ApprovalStatus == "APPROVED" && q.FinalGrade != QualityGradeEnum.REJECTED),
                    FailCount = g.Count(q => q.ApprovalStatus == "REJECTED" || q.FinalGrade == QualityGradeEnum.REJECTED),
                    PassRate = g.Count() > 0 ? Math.Round((decimal)g.Count(q => q.ApprovalStatus == "APPROVED") / g.Count() * 100, 2) : 0
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList()
        };

        return stats;
    }

    public async Task<bool> IsOwnerAsync(int qualityCheckId, int workerId, int farmId)
    {
        return await _context.QualityChecks
            .AnyAsync(q => q.Id == qualityCheckId && q.FarmId == farmId && q.CheckedBy == workerId && !q.IsDeleted);
    }

    public async Task<bool> CanWorkerEditAsync(int qualityCheckId, int workerId, int farmId)
    {
        var qualityCheck = await _context.QualityChecks
            .FirstOrDefaultAsync(q => q.Id == qualityCheckId && q.FarmId == farmId && !q.IsDeleted);
        
        if (qualityCheck == null) return false;
        
        return qualityCheck.CheckedBy == workerId && 
               (qualityCheck.ApprovalStatus == "PENDING" || qualityCheck.ApprovalStatus == "REQUEST_CHANGES");
    }

    public async Task<bool> HasPendingApprovalAsync(int workerId, int farmId)
    {
        return await _context.QualityChecks
            .AnyAsync(q => q.FarmId == farmId && q.CheckedBy == workerId && q.ApprovalStatus == "PENDING" && !q.IsDeleted);
    }
}