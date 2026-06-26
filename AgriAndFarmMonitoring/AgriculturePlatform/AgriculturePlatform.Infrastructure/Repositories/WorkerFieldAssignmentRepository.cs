// AgriculturePlatform.Infrastructure/Repositories/WorkerFieldAssignmentRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class WorkerFieldAssignmentRepository : IWorkerFieldAssignmentRepository
{
    private readonly AppDbContext _context;

    public WorkerFieldAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkerFieldAssignment?> GetByIdAsync(int id, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .Include(a => a.Worker)
            .Include(a => a.Field)
            .FirstOrDefaultAsync(a => a.Id == id && a.FarmId == farmId && !a.IsDeleted);
    }

    public async Task<WorkerFieldAssignment> CreateAsync(WorkerFieldAssignment assignment)
    {
        assignment.CreatedAt = DateTime.UtcNow;
        await _context.WorkerFieldAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAsync(WorkerFieldAssignment assignment)
    {
        assignment.UpdatedAt = DateTime.UtcNow;
        _context.WorkerFieldAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(WorkerFieldAssignment assignment, int deletedBy)
    {
        assignment.IsDeleted = true;
        assignment.DeletedAt = DateTime.UtcNow;
        assignment.DeletedBy = deletedBy;
        assignment.UpdatedAt = DateTime.UtcNow;
        _context.WorkerFieldAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

// AgriculturePlatform.Infrastructure/Repositories/WorkerFieldAssignmentRepository.cs

public async Task<PagedResult<WorkerFieldAssignment>> GetPagedAssignmentsAsync(
    int farmId, int? workerId, int? fieldId, bool? isActive,
    DateTime? assignedDateFrom, DateTime? assignedDateTo,
    DateTime? endDateFrom, DateTime? endDateTo,  // ✅ Added
    PaginationParams paginationParams)
{
    var query = _context.WorkerFieldAssignments
        .Include(a => a.Worker)
        .Include(a => a.Field)
        .Where(a => a.FarmId == farmId && !a.IsDeleted);

    // Filter by worker
    if (workerId.HasValue)
    {
        query = query.Where(a => a.WorkerId == workerId.Value);
    }

    // Filter by field
    if (fieldId.HasValue)
    {
        query = query.Where(a => a.FieldId == fieldId.Value);
    }

    // Filter by active status
    if (isActive.HasValue)
    {
        query = query.Where(a => a.IsActive == isActive.Value);
    }

    // Filter by assigned date range
    if (assignedDateFrom.HasValue)
    {
        var fromDate = assignedDateFrom.Value.Date.ToUniversalTime();
        query = query.Where(a => a.AssignedDate >= fromDate);
    }
    
    if (assignedDateTo.HasValue)
    {
        var toDate = assignedDateTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
        query = query.Where(a => a.AssignedDate <= toDate);
    }

    // ✅ Filter by end date range
    if (endDateFrom.HasValue)
    {
        var fromDate = endDateFrom.Value.Date.ToUniversalTime();
        query = query.Where(a => a.EndDate != null && a.EndDate >= fromDate);
    }
    
    if (endDateTo.HasValue)
    {
        var toDate = endDateTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
        query = query.Where(a => a.EndDate != null && a.EndDate <= toDate);
    }

    // Apply sorting
    if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
    {
        query = paginationParams.IsDescending
            ? query.OrderByDescending(a => EF.Property<object>(a, paginationParams.SortBy))
            : query.OrderBy(a => EF.Property<object>(a, paginationParams.SortBy));
    }
    else
    {
        query = query.OrderByDescending(a => a.AssignedDate);
    }

    var totalCount = await query.CountAsync();

    var items = await query
        .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
        .Take(paginationParams.PageSize)
        .ToListAsync();

    return new PagedResult<WorkerFieldAssignment>
    {
        Items = items,
        TotalCount = totalCount,
        Page = paginationParams.Page,
        PageSize = paginationParams.PageSize
    };
}

    public async Task<List<WorkerFieldAssignment>> GetWorkerAssignedFieldsAsync(int workerId, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .Include(a => a.Field)
            .Where(a => a.WorkerId == workerId && a.FarmId == farmId && a.IsActive && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsFieldAssignedToWorkerAsync(int fieldId, int workerId, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .AnyAsync(a => a.FieldId == fieldId && a.WorkerId == workerId && a.FarmId == farmId && a.IsActive && !a.IsDeleted);
    }

    public async Task<bool> HasWorkerAccessToFieldAsync(int workerId, int fieldId, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .AnyAsync(a => a.WorkerId == workerId 
                           && a.FieldId == fieldId 
                           && a.FarmId == farmId 
                           && a.IsActive == true 
                           && !a.IsDeleted);
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .AnyAsync(a => a.Id == id && a.FarmId == farmId && !a.IsDeleted);
    }

    public async Task<List<Field>> GetFieldsByWorkerAsync(int workerId, int farmId)
    {
        var assignments = await _context.WorkerFieldAssignments
            .Include(a => a.Field)
            .Where(a => a.WorkerId == workerId 
                        && a.FarmId == farmId 
                        && a.IsActive == true 
                        && !a.IsDeleted)
            .ToListAsync();
        
        return assignments
            .Where(a => a.Field != null && !a.Field.IsDeleted)
            .Select(a => a.Field!)
            .ToList();
    }

    public async Task<List<WorkerFieldAssignment>> GetWorkerActiveAssignmentsAsync(int workerId, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .Include(a => a.Field)
            .Where(a => a.WorkerId == workerId 
                        && a.FarmId == farmId 
                        && a.IsActive == true 
                        && !a.IsDeleted)
            .OrderBy(a => a.AssignedDate)
            .ToListAsync();
    }

    public async Task<List<WorkerFieldAssignment>> GetWorkerFieldAssignmentsByFieldAsync(int fieldId, int farmId)
    {
        return await _context.WorkerFieldAssignments
            .Include(a => a.Worker)
            .Where(a => a.FieldId == fieldId && a.FarmId == farmId && a.IsActive && !a.IsDeleted)
            .ToListAsync();
    }
}