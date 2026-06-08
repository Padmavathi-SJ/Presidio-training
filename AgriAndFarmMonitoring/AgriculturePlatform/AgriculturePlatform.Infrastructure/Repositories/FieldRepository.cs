using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class FieldRepository : IFieldRepository
{
    private readonly AppDbContext _context;

    public FieldRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Field?> GetByIdAsync(int id, int farmId, bool includeDeleted = false)
    {
        var query = _context.Fields
            .Include(f => f.Farm)
            .Include(f => f.Admin)
            .Include(f => f.CropCycles)
            .Where(f => f.Id == id && f.FarmId == farmId);
        
        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Field>> GetAllAsync(int farmId, bool includeDeleted = false)
    {
        var query = _context.Fields.Where(f => f.FarmId == farmId);
        
        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }
        
        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Field> CreateAsync(Field field)
    {
        field.CreatedAt = DateTime.UtcNow;
        await _context.Fields.AddAsync(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task UpdateAsync(Field field)
    {
        field.UpdatedAt = DateTime.UtcNow;
        _context.Fields.Update(field);
        await _context.SaveChangesAsync();
    }


    public async Task SoftDeleteAsync(Field field, int deletedBy)
    {
        field.IsDeleted = true;
        field.DeletedAt = DateTime.UtcNow;
        field.DeletedBy = deletedBy;
        field.UpdatedAt = DateTime.UtcNow;
        
        _context.Fields.Update(field);
        await _context.SaveChangesAsync();
    }

    

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.Fields.AnyAsync(f => f.Id == id && f.FarmId == farmId && !f.IsDeleted);
    }

    public async Task<bool> FieldNameExistsAsync(string fieldName, int farmId, int? excludeId = null)
    {
        var query = _context.Fields.Where(f => f.FieldName == fieldName && f.FarmId == farmId && !f.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(f => f.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<PagedResult<Field>> GetPagedAsync(
        int farmId, 
        string? searchTerm, 
        string? soilType, 
        string? status,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var specification = new FieldSpecification(farmId, searchTerm, soilType, status, includeDeleted);
        
        var query = _context.Fields.Where(specification.Criteria!);
        
        // Apply sorting
        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            query = paginationParams.IsDescending
                ? query.OrderByDescending(f => EF.Property<object>(f, paginationParams.SortBy))
                : query.OrderBy(f => EF.Property<object>(f, paginationParams.SortBy));
        }
        else
        {
            query = query.OrderByDescending(f => f.CreatedAt);
        }
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Include(f => f.Farm)
            .Include(f => f.CropCycles)
            .ToListAsync();
        
        return new PagedResult<Field>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

public async Task<int> GetActiveCropsCountAsync(int fieldId)
{
    return await _context.CropCycles
        .CountAsync(c => c.FieldId == fieldId && c.Status == TaskStatusEnum.IN_PROGRESS && !c.IsDeleted);
}

    public async Task<decimal> GetTotalAreaAsync(int farmId, bool includeDeleted = false)
    {
        var query = _context.Fields.Where(f => f.FarmId == farmId);
        
        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }
        
        return await query.SumAsync(f => f.AreaHectares ?? 0);
    }

public async Task<Dictionary<string, int>> GetSoilTypeDistributionAsync(int farmId, bool includeDeleted = false)
{
    var query = _context.Fields
        .Where(f => f.FarmId == farmId && f.SoilType != null);
    
    if (!includeDeleted)
    {
        query = query.Where(f => !f.IsDeleted);
    }
    
    // Group by the enum value, then convert to string after
    var distribution = await query
        .GroupBy(f => f.SoilType)
        .Select(g => new { SoilType = g.Key, Count = g.Count() })
        .ToDictionaryAsync(k => k.SoilType.ToString(), v => v.Count);
    
    return distribution;
}

public async Task<int> GetFieldsCountByStatusAsync(int farmId, string status, bool includeDeleted = false)
{
    var query = _context.Fields.Where(f => f.FarmId == farmId);
    
    if (!string.IsNullOrEmpty(status))
    {
        // FIX: Convert string to enum first, then compare
        if (Enum.TryParse<FieldStatusEnum>(status, true, out var enumStatus))
        {
            query = query.Where(f => f.Status == enumStatus);
        }
        else
        {
            // If invalid status, return 0
            return 0;
        }
    }
    
    if (!includeDeleted)
    {
        query = query.Where(f => !f.IsDeleted);
    }
    
    return await query.CountAsync();
}
   

    public async Task<IEnumerable<Field>> GetFieldsByIdsAsync(IEnumerable<int> ids, int farmId)
    {
        return await _context.Fields
            .Where(f => f.FarmId == farmId && ids.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> BulkCreateAsync(IEnumerable<Field> fields)
    {
        foreach (var field in fields)
    {
        if (field.CreatedAt == default)
            field.CreatedAt = DateTime.UtcNow;
    }
        await _context.Fields.AddRangeAsync(fields);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> BulkSoftDeleteAsync(IEnumerable<int> ids, int farmId, int deletedBy)
    {
        var fields = await _context.Fields
            .Where(f => f.FarmId == farmId && ids.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync();
        
        foreach (var field in fields)
        {
            field.IsDeleted = true;
            field.DeletedAt = DateTime.UtcNow;
            field.DeletedBy = deletedBy;
            field.UpdatedAt = DateTime.UtcNow;
        }
        
        _context.Fields.UpdateRange(fields);
        return await _context.SaveChangesAsync();
    }
}