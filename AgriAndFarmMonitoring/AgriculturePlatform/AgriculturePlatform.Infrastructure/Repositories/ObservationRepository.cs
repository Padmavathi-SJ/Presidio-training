// AgriculturePlatform.Infrastructure/Repositories/ObservationRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class ObservationRepository : IObservationRepository
{
    private readonly AppDbContext _context;

    public ObservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Observation?> GetByIdAsync(int id, int farmId)
    {
        return await _context.Observations
            .Include(o => o.Field)
            .Include(o => o.CropCycle)
            .Include(o => o.Worker)
            .FirstOrDefaultAsync(o => o.Id == id && o.FarmId == farmId && !o.IsDeleted);
    }

    public async Task<Observation> CreateAsync(Observation observation)
    {
        observation.CreatedAt = DateTime.UtcNow;
        await _context.Observations.AddAsync(observation);
        await _context.SaveChangesAsync();
        return observation;
    }

    public async Task UpdateAsync(Observation observation)
    {
        observation.UpdatedAt = DateTime.UtcNow;
        _context.Observations.Update(observation);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Observation observation, int deletedBy)
    {
        observation.IsDeleted = true;
        observation.DeletedAt = DateTime.UtcNow;
        observation.DeletedBy = deletedBy;
        observation.UpdatedAt = DateTime.UtcNow;
        _context.Observations.Update(observation);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.Observations
            .AnyAsync(o => o.Id == id && o.FarmId == farmId && !o.IsDeleted);
    }

    public async Task<PagedResult<Observation>> GetPagedAsync(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        int? workerId,
        string? cropHealth,
        bool? pestDetected,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted,
        PaginationParams paginationParams)
    {
        var specification = new ObservationSpecification(
            farmId, fieldId, cropCycleId, workerId, cropHealth, pestDetected, fromDate, toDate, includeDeleted);

        var query = _context.Observations
            .Include(o => o.Field)
            .Include(o => o.CropCycle)
            .Include(o => o.Worker)
            .Where(specification.Criteria!);

        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            query = paginationParams.IsDescending
                ? query.OrderByDescending(o => EF.Property<object>(o, paginationParams.SortBy))
                : query.OrderBy(o => EF.Property<object>(o, paginationParams.SortBy));
        }
        else
        {
            query = query.OrderByDescending(o => o.ObservationDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Observation>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<Observation>> GetByFieldAsync(int fieldId, int farmId)
    {
        return await _context.Observations
            .Include(o => o.Worker)
            .Where(o => o.FieldId == fieldId && o.FarmId == farmId && !o.IsDeleted)
            .OrderByDescending(o => o.ObservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Observation>> GetByCropCycleAsync(int cropCycleId, int farmId)
    {
        return await _context.Observations
            .Include(o => o.Worker)
            .Where(o => o.CropCycleId == cropCycleId && o.FarmId == farmId && !o.IsDeleted)
            .OrderByDescending(o => o.ObservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Observation>> GetByWorkerAsync(int workerId, int farmId)
    {
        return await _context.Observations
            .Include(o => o.Field)
            .Where(o => o.WorkerId == workerId && o.FarmId == farmId && !o.IsDeleted)
            .OrderByDescending(o => o.ObservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Observation>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Observations
            .Include(o => o.Field)
            .Include(o => o.Worker)
            .Where(o => o.FarmId == farmId && 
                        o.ObservationDate >= fromDate && 
                        o.ObservationDate <= toDate &&
                        !o.IsDeleted)
            .OrderBy(o => o.ObservationDate)
            .ToListAsync();
    }

// AgriculturePlatform.Infrastructure/Repositories/ObservationRepository.cs

// AgriculturePlatform.Infrastructure/Repositories/ObservationRepository.cs
// Update the method to return ObservationStatisticsDto

public async Task<ObservationStatisticsDto> GetPestDetectionStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
{
    var query = _context.Observations.Where(o => o.FarmId == farmId && !o.IsDeleted);

    if (fromDate.HasValue)
        query = query.Where(o => o.ObservationDate >= fromDate.Value);
    if (toDate.HasValue)
        query = query.Where(o => o.ObservationDate <= toDate.Value);

    var observations = await query.ToListAsync();

    return new ObservationStatisticsDto
    {
        TotalObservations = observations.Count,
        ObservationsWithPest = observations.Count(o => o.PestDetected),
        ObservationsWithoutPest = observations.Count(o => !o.PestDetected),
        PestTypeDistribution = observations.Where(o => !string.IsNullOrWhiteSpace(o.PestType))
            .GroupBy(o => o.PestType!)
            .ToDictionary(g => g.Key, g => g.Count()),
        CropHealthDistribution = observations.Where(o => o.CropHealth.HasValue)
            .GroupBy(o => o.CropHealth!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count()),
        ObservationsByField = observations.Where(o => o.Field != null)
            .GroupBy(o => o.Field!.FieldName)
            .ToDictionary(g => g.Key, g => g.Count()),
        ObservationsByWorker = observations.Where(o => o.Worker != null)
            .GroupBy(o => o.Worker!.Name)
            .ToDictionary(g => g.Key, g => g.Count()),
        RecentTrend = observations
            .GroupBy(o => o.ObservationDate.Date)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .Select(g => new DailyObservationTrendDto
            {
                Date = g.Key,
                TotalCount = g.Count(),
                PestCount = g.Count(o => o.PestDetected)
            })
            .OrderBy(t => t.Date)
            .ToList()
    };
}
  
  
    public async Task<Dictionary<string, int>> GetPestTypeDistributionAsync(int farmId)
    {
        var observations = await _context.Observations
            .Where(o => o.FarmId == farmId && o.PestDetected && !string.IsNullOrWhiteSpace(o.PestType) && !o.IsDeleted)
            .ToListAsync();

        return observations
            .GroupBy(o => o.PestType!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<bool> IsOwnerAsync(int observationId, int workerId, int farmId)
    {
        return await _context.Observations
            .AnyAsync(o => o.Id == observationId && o.WorkerId == workerId && o.FarmId == farmId && !o.IsDeleted);
    }
}

