// Infrastructure/Repositories/HarvestRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class HarvestRepository : IHarvestRepository
{
    private readonly AppDbContext _context;

    public HarvestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Harvest?> GetByIdAsync(int id, int farmId)
    {
        return await _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Include(h => h.Harvester)
            .Include(h => h.Submitter)
            .Include(h => h.Approver)
            .Include(h => h.QualityChecks)
            .FirstOrDefaultAsync(h => h.Id == id && h.FarmId == farmId && !h.IsDeleted);
    }

// Infrastructure/Repositories/HarvestRepository.cs

public async Task<Harvest> CreateAsync(Harvest harvest)
{
    // ✅ Clean image paths before saving
    harvest.ImagePath = CleanPath(harvest.ImagePath);
    harvest.ThumbnailPath = CleanPath(harvest.ThumbnailPath);
    if (harvest.AdditionalImagePaths?.Any() == true)
    {
        harvest.AdditionalImagePaths = harvest.AdditionalImagePaths
            .Select(CleanPath)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
    }
    
    harvest.CreatedAt = DateTime.UtcNow;
    await _context.Harvests.AddAsync(harvest);
    await _context.SaveChangesAsync();
    return harvest;
}

private string? CleanPath(string? path)
{
    if (string.IsNullOrEmpty(path))
        return null;
    
    // ✅ Remove full URL prefix if present
    if (path.StartsWith("http://") || path.StartsWith("https://"))
    {
        try
        {
            var uri = new Uri(path);
            path = uri.AbsolutePath.TrimStart('/');
        }
        catch
        {
            // If URI parsing fails, try to clean manually
            var parts = path.Split(new[] { "uploads/" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                path = parts[parts.Length - 1];
            }
        }
    }
    
    // Remove "uploads/" prefix if present
    path = path.Replace("uploads/", "");
    path = path.TrimStart('/');
    
    return string.IsNullOrEmpty(path) ? null : path;
}

    public async Task UpdateAsync(Harvest harvest)
    {
        harvest.UpdatedAt = DateTime.UtcNow;
        _context.Harvests.Update(harvest);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Harvest harvest, int deletedBy)
    {
        harvest.IsDeleted = true;
        harvest.DeletedAt = DateTime.UtcNow;
        harvest.DeletedBy = deletedBy;
        harvest.UpdatedAt = DateTime.UtcNow;
        _context.Harvests.Update(harvest);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.Harvests
            .AnyAsync(h => h.Id == id && h.FarmId == farmId && !h.IsDeleted);
    }

    public async Task<PagedResult<Harvest>> GetPagedAsync(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        int? workerId,
        string? approvalStatus,
        string? qualityGrade,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var query = _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Include(h => h.Harvester)
            .Include(h => h.Submitter)
            .Include(h => h.Approver)
            .Where(h => h.FarmId == farmId);

        if (!includeDeleted)
            query = query.Where(h => !h.IsDeleted);

        if (fieldId.HasValue)
            query = query.Where(h => h.FieldId == fieldId.Value);
        
        if (cropCycleId.HasValue)
            query = query.Where(h => h.CropCycleId == cropCycleId.Value);
        
        if (workerId.HasValue)
            query = query.Where(h => h.HarvestedBy == workerId.Value || h.SubmittedBy == workerId.Value);
        
        if (!string.IsNullOrWhiteSpace(approvalStatus))
            query = query.Where(h => h.ApprovalStatus == approvalStatus);
        
        if (!string.IsNullOrWhiteSpace(qualityGrade) && Enum.TryParse<QualityGradeEnum>(qualityGrade, true, out var grade))
            query = query.Where(h => h.QualityGrade == grade);
        
        if (fromDate.HasValue)
            query = query.Where(h => h.HarvestDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(h => h.HarvestDate <= toDate.Value);

        query = paginationParams.IsDescending
            ? query.OrderByDescending(h => EF.Property<object>(h, paginationParams.SortBy ?? "HarvestDate"))
            : query.OrderBy(h => EF.Property<object>(h, paginationParams.SortBy ?? "HarvestDate"));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Harvest>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<Harvest>> GetByFieldAsync(int fieldId, int farmId)
    {
        return await _context.Harvests
            .Include(h => h.Harvester)
            .Include(h => h.Approver)
            .Where(h => h.FieldId == fieldId && h.FarmId == farmId && !h.IsDeleted)
            .OrderByDescending(h => h.HarvestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Harvest>> GetByCropCycleAsync(int cropCycleId, int farmId)
    {
        return await _context.Harvests
            .Include(h => h.Harvester)
            .Where(h => h.CropCycleId == cropCycleId && h.FarmId == farmId && !h.IsDeleted)
            .OrderByDescending(h => h.HarvestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Harvest>> GetByWorkerAsync(int workerId, int farmId)
    {
        return await _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Include(h => h.Approver)
            .Where(h => (h.HarvestedBy == workerId || h.SubmittedBy == workerId) && h.FarmId == farmId && !h.IsDeleted)
            .OrderByDescending(h => h.HarvestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Harvest>> GetPendingApprovalsAsync(int farmId)
    {
        return await _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Include(h => h.Harvester)
            .Include(h => h.Submitter)
            .Where(h => h.FarmId == farmId && h.ApprovalStatus == "PENDING" && !h.IsDeleted)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Harvest>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Where(h => h.FarmId == farmId && h.HarvestDate >= fromDate && h.HarvestDate <= toDate && !h.IsDeleted)
            .OrderBy(h => h.HarvestDate)
            .ToListAsync();
    }

    public async Task<YieldStatisticsDto> GetYieldStatisticsAsync(int farmId, int? cropCycleId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Harvests
            .Include(h => h.Field)
            .Include(h => h.CropCycle)
            .Where(h => h.FarmId == farmId && h.ApprovalStatus == "APPROVED" && !h.IsDeleted);

        if (cropCycleId.HasValue)
            query = query.Where(h => h.CropCycleId == cropCycleId.Value);
        
        if (fromDate.HasValue)
            query = query.Where(h => h.HarvestDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(h => h.HarvestDate <= toDate.Value);

        var harvests = await query.ToListAsync();

        var stats = new YieldStatisticsDto
        {
            TotalHarvests = harvests.Count,
            TotalYieldKg = harvests.Sum(h => h.QuantityKg),
            YieldByField = harvests.Where(h => h.Field != null)
                .GroupBy(h => h.Field!.FieldName)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.QuantityKg)),
            YieldByCropType = harvests.Where(h => h.CropCycle != null && h.CropCycle.CropType != null)
                .GroupBy(h => h.CropCycle!.CropType!.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(h => h.QuantityKg)),
            QualityDistribution = harvests.Where(h => h.QualityGrade.HasValue)
                .GroupBy(h => h.QualityGrade!.Value.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            HarvestMethodDistribution = harvests.Where(h => h.HarvestMethod.HasValue)
                .GroupBy(h => h.HarvestMethod!.Value.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TotalValue = harvests.Sum(h => h.TotalValue ?? 0),
            AveragePricePerKg = harvests.Where(h => h.PricePerKg.HasValue && h.QuantityKg > 0)
                .Average(h => h.PricePerKg ?? 0),
            MonthlyTrend = harvests
                .GroupBy(h => new { h.HarvestDate.Year, h.HarvestDate.Month })
                .Select(g => new MonthlyYieldDto
                {
                    Year = g.Key.Year,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    YieldKg = g.Sum(h => h.QuantityKg),
                    HarvestCount = g.Count(),
                    AveragePrice = g.Average(h => h.PricePerKg ?? 0)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList()
        };

        stats.AverageYieldPerHectare = stats.TotalYieldKg / (harvests.Select(h => h.Field?.AreaHectares).FirstOrDefault() ?? 1);

        return stats;
    }

    public async Task<Dictionary<string, decimal>> GetYieldByFieldAsync(int farmId, int year)
    {
        var harvests = await _context.Harvests
            .Include(h => h.Field)
            .Where(h => h.FarmId == farmId && h.HarvestDate.Year == year && h.ApprovalStatus == "APPROVED" && !h.IsDeleted)
            .ToListAsync();

        return harvests
            .Where(h => h.Field != null)
            .GroupBy(h => h.Field!.FieldName)
            .ToDictionary(g => g.Key, g => g.Sum(h => h.QuantityKg));
    }

    public async Task<decimal> GetTotalYieldForSeasonAsync(int farmId, int cropCycleId)
    {
        return await _context.Harvests
            .Where(h => h.FarmId == farmId && h.CropCycleId == cropCycleId && h.ApprovalStatus == "APPROVED" && !h.IsDeleted)
            .SumAsync(h => h.QuantityKg);
    }

    public async Task<bool> IsOwnerAsync(int harvestId, int workerId, int farmId)
    {
        return await _context.Harvests
            .AnyAsync(h => h.Id == harvestId && h.FarmId == farmId && 
                          (h.HarvestedBy == workerId || h.SubmittedBy == workerId) && 
                          !h.IsDeleted);
    }

    public async Task<bool> CanWorkerEditAsync(int harvestId, int workerId, int farmId)
    {
        var harvest = await _context.Harvests
            .FirstOrDefaultAsync(h => h.Id == harvestId && h.FarmId == farmId && !h.IsDeleted);
        
        if (harvest == null) return false;
        
        // Worker can edit only if they are the owner AND status is PENDING or REQUEST_CHANGES
        return (harvest.HarvestedBy == workerId || harvest.SubmittedBy == workerId) &&
               (harvest.ApprovalStatus == "PENDING" || harvest.ApprovalStatus == "REQUEST_CHANGES");
    }

    public async Task<bool> HasPendingApprovalAsync(int workerId, int farmId)
    {
        return await _context.Harvests
            .AnyAsync(h => h.FarmId == farmId && 
                          (h.HarvestedBy == workerId || h.SubmittedBy == workerId) && 
                          h.ApprovalStatus == "PENDING" && 
                          !h.IsDeleted);
    }
}