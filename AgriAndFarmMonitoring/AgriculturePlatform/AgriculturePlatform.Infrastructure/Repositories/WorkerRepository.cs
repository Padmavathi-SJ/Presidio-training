// AgriculturePlatform.Infrastructure/Repositories/WorkerRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private readonly AppDbContext _context;

    public WorkerRepository(AppDbContext context)
    {
        _context = context;
    }

    // =============================================
    // Basic CRUD Operations
    // =============================================

    public async Task<Worker?> GetByIdAsync(int id, int farmId, bool includeDeleted = false)
    {
        var query = _context.Workers
            .Include(w => w.Farm)
            .Include(w => w.Admin)
            .Where(w => w.Id == id && w.FarmId == farmId);

        if (!includeDeleted)
        {
            query = query.Where(w => !w.IsDeleted);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Worker?> GetByEmailAsync(string email)
{
    return await _context.Workers
        .Include(w => w.Farm)
        .Where(w => w.Email == email && !w.IsDeleted)
        .FirstOrDefaultAsync();
}

    public async Task<IEnumerable<Worker>> GetAllAsync(int farmId, bool includeDeleted = false)
    {
        var query = _context.Workers
            .Include(w => w.Farm)
            .Where(w => w.FarmId == farmId);

        if (!includeDeleted)
        {
            query = query.Where(w => !w.IsDeleted);
        }

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<Worker> CreateAsync(Worker worker)
    {
        worker.CreatedAt = DateTime.UtcNow;
        await _context.Workers.AddAsync(worker);
        await _context.SaveChangesAsync();
        return worker;
    }

    public async Task UpdateAsync(Worker worker)
    {
        worker.UpdatedAt = DateTime.UtcNow;
        _context.Workers.Update(worker);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Worker worker, int deletedBy)
    {
        worker.IsDeleted = true;
        worker.DeletedAt = DateTime.UtcNow;
        worker.DeletedBy = deletedBy;
        worker.UpdatedAt = DateTime.UtcNow;
        _context.Workers.Update(worker);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.Workers
            .AnyAsync(w => w.Id == id && w.FarmId == farmId && !w.IsDeleted);
    }

    // =============================================
    // Query Methods
    // =============================================

    public async Task<bool> EmailExistsAsync(string email, int farmId, int? excludeId = null)
    {
        var query = _context.Workers
            .Where(w => w.Email == email && w.FarmId == farmId && !w.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    // =============================================
    // Filtering & Pagination
    // =============================================

    public async Task<PagedResult<Worker>> GetPagedAsync(
        int farmId,
        string? name,
        string? email,
        string? role,
        bool? isActive,
        DateTime? hireDateFrom,
        DateTime? hireDateTo,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var specification = new WorkerSpecification(
            farmId, name, email, role, isActive, hireDateFrom, hireDateTo, includeDeleted);

        var query = _context.Workers.Where(specification.Criteria!);

        // Apply sorting — use a whitelist to avoid EF.Property translation failures
        // caused by case mismatches or invalid property names.
        query = paginationParams.SortBy?.ToLowerInvariant() switch
        {
            "name"        => paginationParams.IsDescending ? query.OrderByDescending(w => w.Name)        : query.OrderBy(w => w.Name),
            "email"       => paginationParams.IsDescending ? query.OrderByDescending(w => w.Email)       : query.OrderBy(w => w.Email),
            "role"        => paginationParams.IsDescending ? query.OrderByDescending(w => w.Role)        : query.OrderBy(w => w.Role),
            "isactive"    => paginationParams.IsDescending ? query.OrderByDescending(w => w.IsActive)    : query.OrderBy(w => w.IsActive),
            "hiredate"    => paginationParams.IsDescending ? query.OrderByDescending(w => w.HireDate)    : query.OrderBy(w => w.HireDate),
            "lastloginat" => paginationParams.IsDescending ? query.OrderByDescending(w => w.LastLoginAt) : query.OrderBy(w => w.LastLoginAt),
            "updatedat"   => paginationParams.IsDescending ? query.OrderByDescending(w => w.UpdatedAt)   : query.OrderBy(w => w.UpdatedAt),
            _             => query.OrderByDescending(w => w.CreatedAt)   // "createdat" and any unknown value
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Include(w => w.Farm)
            .ToListAsync();

        return new PagedResult<Worker>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    // =============================================
    // Statistics
    // =============================================

    public async Task<int> GetActiveWorkersCountAsync(int farmId)
    {
        return await _context.Workers
            .CountAsync(w => w.FarmId == farmId && w.IsActive && !w.IsDeleted);
    }

    public async Task<Dictionary<string, int>> GetWorkersByRoleDistributionAsync(int farmId)
    {
        var distribution = await _context.Workers
            .Where(w => w.FarmId == farmId && !w.IsDeleted && w.Role != null)
            .GroupBy(w => w.Role!)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Role, v => v.Count);

        return distribution;
    }

    public async Task<DateTime?> GetLastLoginAsync(int workerId)
    {
        var lastLogin = await _context.AuditLogs
            .Where(a => a.WorkerId == workerId && a.Action == "LOGIN")
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        return lastLogin;
    }

    // =============================================
    // Password Management
    // =============================================

    public async Task UpdatePasswordAsync(int workerId, string passwordHash)
    {
        var worker = await _context.Workers.FindAsync(workerId);
        if (worker != null)
        {
            worker.PasswordHash = passwordHash;
            worker.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // =============================================
    // Login Tracking
    // =============================================

    public async Task RecordLoginAsync(int workerId, string ipAddress)
    {
        var auditLog = new AuditLog
        {
            WorkerId = workerId,
            Action = "LOGIN",
            EntityType = "Worker",
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

// AgriculturePlatform.Infrastructure/Repositories/WorkerRepository.cs
// Add these methods to the existing repository

public async Task<Worker?> GetWorkerWithFarmAsync(int workerId, int farmId)
{
    return await _context.Workers
        .Include(w => w.Farm)
        .Include(w => w.Admin)
        .Where(w => w.Id == workerId && w.FarmId == farmId && !w.IsDeleted)
        .FirstOrDefaultAsync();
}

public async Task<bool> UpdateWorkerProfileAsync(Worker worker)
{
    worker.UpdatedAt = DateTime.UtcNow;
    _context.Workers.Update(worker);
    return await _context.SaveChangesAsync() > 0;
}

public async Task<bool> UpdateWorkerPasswordAsync(int workerId, string newPasswordHash)
{
    var worker = await _context.Workers.FindAsync(workerId);
    if (worker == null) return false;
    
    worker.PasswordHash = newPasswordHash;
    worker.UpdatedAt = DateTime.UtcNow;
    return await _context.SaveChangesAsync() > 0;
}

}