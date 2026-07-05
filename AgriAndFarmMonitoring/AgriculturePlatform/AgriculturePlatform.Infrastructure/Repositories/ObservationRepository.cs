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
        DateTime? fromDate, 
        DateTime? toDate, 
        string? validationStatus,
        bool includeDeleted, 
        PaginationParams paginationParams)
    {
        var specification = new ObservationSpecification(
            farmId, fieldId, cropCycleId, workerId, cropHealth, 
            fromDate, toDate, validationStatus, includeDeleted);

        var query = _context.Observations
            .Include(o => o.Field)
            .Include(o => o.CropCycle)
            .Include(o => o.Worker)
            .Include(o => o.Validator) 
            .Where(specification.Criteria!);

        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            var isDescending = paginationParams.IsDescending;
            query = paginationParams.SortBy switch
            {
                "observationDate" => isDescending ? query.OrderByDescending(o => o.ObservationDate) : query.OrderBy(o => o.ObservationDate),
                "cropHealth" => isDescending ? query.OrderByDescending(o => o.CropHealth) : query.OrderBy(o => o.CropHealth),
                "validationStatus" => isDescending ? query.OrderByDescending(o => o.ValidationStatus) : query.OrderBy(o => o.ValidationStatus),
                "createdAt" => isDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
                "fieldName" => isDescending ? query.OrderByDescending(o => o.Field.FieldName) : query.OrderBy(o => o.Field.FieldName),
                "workerName" => isDescending ? query.OrderByDescending(o => o.Worker.Name) : query.OrderBy(o => o.Worker.Name),
                _ => isDescending 
                    ? query.OrderByDescending(o => EF.Property<object>(o, char.ToUpper(paginationParams.SortBy[0]) + paginationParams.SortBy.Substring(1)))
                    : query.OrderBy(o => EF.Property<object>(o, char.ToUpper(paginationParams.SortBy[0]) + paginationParams.SortBy.Substring(1)))
            };
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

public async Task<ObservationStatisticsDto> GetPestDetectionStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
{
    var query = _context.Observations
        .Include(o => o.Field)
        .Include(o => o.Worker)
        .Include(o => o.CropCycle)
        .Where(o => o.FarmId == farmId && !o.IsDeleted);

    if (fromDate.HasValue)
        query = query.Where(o => o.ObservationDate >= fromDate.Value);
    if (toDate.HasValue)
        query = query.Where(o => o.ObservationDate <= toDate.Value);

    var observations = await query.ToListAsync();

    // Standardize pest types - convert to Title Case and group
    var pestTypeDist = observations
        .Where(o => !string.IsNullOrWhiteSpace(o.PestType))
        .GroupBy(o => StandardizePestType(o.PestType!))
        .ToDictionary(g => g.Key, g => g.Count());

    // Standardize crop health values
    var cropHealthDist = observations
        .Where(o => o.CropHealth.HasValue)
        .GroupBy(o => FormatCropHealth(o.CropHealth!.Value))
        .ToDictionary(g => g.Key, g => g.Count());

    // Get field names properly
    var observationsByField = observations
        .Where(o => o.Field != null)
        .GroupBy(o => o.Field!.FieldName)
        .ToDictionary(g => g.Key, g => g.Count());

    // Get worker names properly
    var observationsByWorker = observations
        .Where(o => o.Worker != null)
        .GroupBy(o => o.Worker!.Name)
        .ToDictionary(g => g.Key, g => g.Count());

    return new ObservationStatisticsDto
    {
        TotalObservations = observations.Count,
        ObservationsWithPest = observations.Count(o => !string.IsNullOrWhiteSpace(o.PestType)),
        ObservationsWithoutPest = observations.Count(o => string.IsNullOrWhiteSpace(o.PestType)),
        PestTypeDistribution = pestTypeDist,
        CropHealthDistribution = cropHealthDist,
        ObservationsByField = observationsByField,
        ObservationsByWorker = observationsByWorker,
        RecentTrend = observations
            .GroupBy(o => o.ObservationDate.Date)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .Select(g => new DailyObservationTrendDto
            {
                Date = g.Key,
                TotalCount = g.Count(),
                PestCount = g.Count(o => !string.IsNullOrWhiteSpace(o.PestType))
            })
            .OrderBy(t => t.Date)
            .ToList()
    };
}

private string StandardizePestType(string pestType)
{
    if (string.IsNullOrWhiteSpace(pestType)) return string.Empty;
    
    // Convert to Title Case and trim
    var standardized = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pestType.ToLower().Trim());
    
    // Map common variations
    return standardized switch
    {
        "Aphid" or "Aphids" => "Aphids",
        "Beetle" or "Beetles" => "Beetles",
        "Caterpillar" or "Caterpillars" => "Caterpillars",
        "Mite" or "Mites" => "Mites",
        "Whitefly" or "Whiteflies" => "Whiteflies",
        "Thrip" or "Thrips" => "Thrips",
        _ => standardized
    };
}

private string FormatCropHealth(CropHealthEnum health)
{
    return health switch
    {
        CropHealthEnum.EXCELLENT => "Excellent",
        CropHealthEnum.GOOD => "Good",
        CropHealthEnum.AVERAGE => "Average",  // ← Fixed: AVERAGE instead of FAIR
        CropHealthEnum.POOR => "Poor",
        CropHealthEnum.CRITICAL => "Critical",
        _ => health.ToString()
    };
}    public async Task<Dictionary<string, int>> GetPestTypeDistributionAsync(int farmId)
    {
        var observations = await _context.Observations
            .Where(o => o.FarmId == farmId && !string.IsNullOrWhiteSpace(o.PestType) && !o.IsDeleted)
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

public async Task<IEnumerable<Observation>> GetPendingValidationsAsync(int farmId)
{
    return await _context.Observations
        .Include(o => o.Field)
        .Include(o => o.Worker)
        .Where(o => o.FarmId == farmId && 
                    o.ValidationStatus == "pending" && 
                    !o.IsDeleted)
        .OrderBy(o => o.ObservationDate)
        .ToListAsync();
}

public async Task<IEnumerable<Observation>> GetQuestionedObservationsAsync(int farmId)
{
    return await _context.Observations
        .Include(o => o.Field)
        .Include(o => o.Worker)
        .Where(o => o.FarmId == farmId && 
                    o.ValidationStatus == "questioned" && 
                    !o.IsDeleted)
        .OrderBy(o => o.ObservationDate)
        .ToListAsync();
}

public async Task<int> CountByValidationStatusAsync(int farmId, string? validationStatus)
{
    var query = _context.Observations
        .Where(o => o.FarmId == farmId && !o.IsDeleted);
    
    if (!string.IsNullOrWhiteSpace(validationStatus))
    {
        query = query.Where(o => o.ValidationStatus == validationStatus);
    }
    
    return await query.CountAsync();
}

}

