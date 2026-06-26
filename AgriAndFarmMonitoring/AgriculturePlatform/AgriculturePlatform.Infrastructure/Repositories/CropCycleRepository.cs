// AgriculturePlatform.Infrastructure/Repositories/CropCycleRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class CropCycleRepository : ICropCycleRepository
{
    private readonly AppDbContext _context;

    public CropCycleRepository(AppDbContext context)
    {
        _context = context;
    }

    // =============================================
    // Basic CRUD Operations
    // =============================================

    public async Task<CropCycle?> GetByIdAsync(int id, int farmId, bool includeDeleted = false)
    {
        var query = _context.CropCycles
            .Include(c => c.Field)
            .Include(c => c.Farm)
            .Where(c => c.Id == id && c.FarmId == farmId);

        if (!includeDeleted)
        {
            query = query.Where(c => !c.IsDeleted);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CropCycle>> GetAllAsync(int farmId, bool includeDeleted = false)
    {
        var query = _context.CropCycles
            .Include(c => c.Field)
            .Where(c => c.FarmId == farmId);

        if (!includeDeleted)
        {
            query = query.Where(c => !c.IsDeleted);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CropCycle> CreateAsync(CropCycle cropCycle)
    {
        cropCycle.CreatedAt = DateTime.UtcNow;
        await _context.CropCycles.AddAsync(cropCycle);
        await _context.SaveChangesAsync();
        return cropCycle;
    }

    public async Task UpdateAsync(CropCycle cropCycle)
    {
        cropCycle.UpdatedAt = DateTime.UtcNow;
        _context.CropCycles.Update(cropCycle);
        await _context.SaveChangesAsync();
        
    }

    public async Task SoftDeleteAsync(CropCycle cropCycle, int deletedBy)
    {
        cropCycle.IsDeleted = true;
        cropCycle.DeletedAt = DateTime.UtcNow;
        cropCycle.DeletedBy = deletedBy;
        cropCycle.UpdatedAt = DateTime.UtcNow;
        _context.CropCycles.Update(cropCycle);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.CropCycles
            .AnyAsync(c => c.Id == id && c.FarmId == farmId && !c.IsDeleted);
    }

    // =============================================
    // Business Logic Methods
    // =============================================

    public async Task<bool> HasActiveCropCycleAsync(int fieldId, int? excludeId = null)
    {
        var query = _context.CropCycles
            .Where(c => c.FieldId == fieldId && c.Status == TaskStatusEnum.IN_PROGRESS && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<PagedResult<CropCycle>> GetPagedAsync(
        int farmId,
        int? fieldId,
        string? cropType,
        string? growthStage,
        string? status,
        DateTime? expectedHarvestDateFrom,
        DateTime? expectedHarvestDateTo,
        bool? activeOnly,
        bool? overdueOnly,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var specification = new CropCycleSpecification(
            farmId, fieldId, cropType, growthStage, status,
            expectedHarvestDateFrom, expectedHarvestDateTo,
            activeOnly, overdueOnly, includeDeleted);

        var query = _context.CropCycles.Where(specification.Criteria!);

      // ✅ FIX: Map sort column to proper property names
    if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
    {
        var sortColumn = paginationParams.SortBy switch
        {
            "id" => "Id",
            "fieldId" => "FieldId",
            "cropType" => "CropType",
            "growthStage" => "GrowthStage",
            "status" => "Status",
            "plantingDate" => "PlantingDate",
            "expectedHarvestDate" => "ExpectedHarvestDate",
            "createdAt" => "CreatedAt",  // ✅ Map createdAt to CreatedAt
            "updatedAt" => "UpdatedAt",  // ✅ Map updatedAt to UpdatedAt
            _ => paginationParams.SortBy
        };
query = paginationParams.IsDescending
            ? query.OrderByDescending(c => EF.Property<object>(c, sortColumn))
            : query.OrderBy(c => EF.Property<object>(c, sortColumn));
    }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Include(c => c.Field)
            .ToListAsync();

        return new PagedResult<CropCycle>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<int> GetActiveCountByFieldAsync(int fieldId)
    {
        return await _context.CropCycles
            .CountAsync(c => c.FieldId == fieldId && c.Status == TaskStatusEnum.IN_PROGRESS && !c.IsDeleted);
    }

    public async Task<IEnumerable<CropCycle>> GetOverdueCropCyclesAsync(int farmId)
    {
        var today = DateTime.UtcNow.Date;
        
        return await _context.CropCycles
            .Include(c => c.Field)
            .Where(c => c.FarmId == farmId && 
                       c.Status == TaskStatusEnum.IN_PROGRESS && 
                       c.ExpectedHarvestDate < today &&
                       !c.IsDeleted)
            .OrderBy(c => c.ExpectedHarvestDate)
            .ToListAsync();
    }
}