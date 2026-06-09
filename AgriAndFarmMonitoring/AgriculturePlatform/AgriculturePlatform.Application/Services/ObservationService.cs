// AgriculturePlatform.Application/Services/ObservationService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class ObservationService : IObservationService
{
    private readonly IObservationRepository _observationRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public ObservationService(
        IObservationRepository observationRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _observationRepository = observationRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    // =============================================
    // WORKER OPERATIONS
    // =============================================

    public async Task<ApiResponse<ObservationDto>> CreateObservationAsync(CreateObservationDto dto, int farmId, int workerId)
    {
        // Validate field
        var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
        if (field == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId} not found");
        }

        // Validate crop cycle if provided
        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
            {
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId} not found");
            }
        }

        var observation = new Observation
        {
            FarmId = farmId,
            FieldId = dto.FieldId,
            CropCycleId = dto.CropCycleId,
            WorkerId = workerId,
            ObservationDate = dto.ObservationDate.ToUniversalTime(),
            CropHealth = !string.IsNullOrWhiteSpace(dto.CropHealth) 
                ? Enum.Parse<CropHealthEnum>(dto.CropHealth, true) 
                : null,
            PestDetected = dto.PestDetected,
            PestType = dto.PestType,
            Notes = dto.Notes,
            CreatedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _observationRepository.CreateAsync(observation);
        
        // FIXED: LogCreateAsync with correct parameters (6 parameters + 2 optional)
        await _auditLogService.LogCreateAsync(
            farmId,           // farmId
            null,             // adminId (null for worker)
            "Observation",    // entityType
            created.Id,       // entityId
            created,          // entity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        var result = _mapper.Map<ObservationDto>(created);
        result.FieldName = field.FieldName;
        
        return ApiResponse<ObservationDto>.Ok(result, "Observation created successfully");
    }

    public async Task<ApiResponse<ObservationDto>> UpdateOwnObservationAsync(int id, UpdateObservationDto dto, int workerId, int farmId)
    {
        // Verify ownership
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<ObservationDto>.Fail("You don't have permission to update this observation");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }

        var oldObservation = _mapper.Map<Observation>(observation);

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(dto.CropHealth))
            observation.CropHealth = Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
        if (dto.PestDetected.HasValue)
            observation.PestDetected = dto.PestDetected.Value;
        if (!string.IsNullOrWhiteSpace(dto.PestType))
            observation.PestType = dto.PestType;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            observation.Notes = dto.Notes;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = workerId;

        await _observationRepository.UpdateAsync(observation);
        
        // FIXED: LogUpdateAsync with correct parameters (7 parameters + 2 optional)
        await _auditLogService.LogUpdateAsync(
            farmId,           // farmId
            null,             // adminId (null for worker)
            "Observation",    // entityType
            observation.Id,   // entityId
            oldObservation,   // oldEntity
            observation,      // newEntity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        var result = _mapper.Map<ObservationDto>(observation);
        return ApiResponse<ObservationDto>.Ok(result, "Observation updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteOwnObservationAsync(int id, int workerId, int farmId)
    {
        // Verify ownership
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<bool>.Fail("You don't have permission to delete this observation");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<bool>.Fail($"Observation with ID {id} not found");
        }

        await _observationRepository.SoftDeleteAsync(observation, workerId);
        
        // FIXED: LogSoftDeleteAsync with correct parameters (6 parameters + 2 optional)
        await _auditLogService.LogSoftDeleteAsync(
            farmId,           // farmId
            null,             // adminId (null for worker)
            "Observation",    // entityType
            observation.Id,   // entityId
            observation,      // entity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        return ApiResponse<bool>.Ok(true, "Observation deleted successfully");
    }

    // =============================================
    // ADMIN OPERATIONS
    // =============================================

    public async Task<ApiResponse<ObservationDto>> UpdateObservationAsync(int id, UpdateObservationDto dto, int farmId, int adminId)
    {
        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }

        var oldObservation = _mapper.Map<Observation>(observation);

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(dto.CropHealth))
            observation.CropHealth = Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
        if (dto.PestDetected.HasValue)
            observation.PestDetected = dto.PestDetected.Value;
        if (!string.IsNullOrWhiteSpace(dto.PestType))
            observation.PestType = dto.PestType;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            observation.Notes = dto.Notes;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = adminId;

        await _observationRepository.UpdateAsync(observation);
        
        // FIXED: LogUpdateAsync with correct parameters
        await _auditLogService.LogUpdateAsync(
            farmId,           // farmId
            adminId,          // adminId
            "Observation",    // entityType
            observation.Id,   // entityId
            oldObservation,   // oldEntity
            observation,      // newEntity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        var result = _mapper.Map<ObservationDto>(observation);
        return ApiResponse<ObservationDto>.Ok(result, "Observation updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteObservationAsync(int id, int farmId, int adminId)
    {
        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<bool>.Fail($"Observation with ID {id} not found");
        }

        await _observationRepository.SoftDeleteAsync(observation, adminId);
        
        // FIXED: LogSoftDeleteAsync with correct parameters
        await _auditLogService.LogSoftDeleteAsync(
            farmId,           // farmId
            adminId,          // adminId
            "Observation",    // entityType
            observation.Id,   // entityId
            observation,      // entity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        return ApiResponse<bool>.Ok(true, "Observation deleted successfully");
    }

    public async Task<ApiResponse<PagedResult<ObservationDto>>> GetAllObservationsAsync(ObservationFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _observationRepository.GetPagedAsync(
            farmId,
            filter.FieldId,
            filter.CropCycleId,
            filter.WorkerId,
            filter.CropHealth,
            filter.PestDetected,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<ObservationDto>>(pagedResult.Items);
        
        var result = new PagedResult<ObservationDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<ObservationDto>>.Ok(result);
    }

    public async Task<ApiResponse<ObservationDto>> GetObservationByIdAsync(int id, int farmId)
    {
        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }

        var result = _mapper.Map<ObservationDto>(observation);
        result.FieldName = observation.Field?.FieldName ?? string.Empty;
        result.WorkerName = observation.Worker?.Name ?? string.Empty;
        result.CropType = observation.CropCycle?.CropType?.ToString() ?? string.Empty;
        
        return ApiResponse<ObservationDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByFieldAsync(int fieldId, int farmId)
    {
        var observations = await _observationRepository.GetByFieldAsync(fieldId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByCropCycleAsync(int cropCycleId, int farmId)
    {
        var observations = await _observationRepository.GetByCropCycleAsync(cropCycleId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByWorkerAsync(int workerId, int farmId)
    {
        var observations = await _observationRepository.GetByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate)
    {
        var observations = await _observationRepository.GetByDateRangeAsync(farmId, fromDate.ToUniversalTime(), toDate.ToUniversalTime());
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ObservationStatisticsDto>> GetPestStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _observationRepository.GetPestDetectionStatisticsAsync(farmId, fromDate, toDate);
        
        var result = new ObservationStatisticsDto
        {
            TotalObservations = stats.TotalObservations,
            ObservationsWithPest = stats.ObservationsWithPest,
            ObservationsWithoutPest = stats.ObservationsWithoutPest,
            PestTypeDistribution = stats.PestTypeDistribution,
            CropHealthDistribution = stats.CropHealthDistribution,
            ObservationsByField = stats.ObservationsByField,
            ObservationsByWorker = stats.ObservationsByWorker,
            RecentTrend = stats.RecentTrend.Select(t => new DailyObservationTrendDto
            {
                Date = t.Date,
                TotalCount = t.TotalCount,
                PestCount = t.PestCount
            }).ToList()
        };

        return ApiResponse<ObservationStatisticsDto>.Ok(result);
    }

    public async Task<bool> ValidateObservationOwnershipAsync(int observationId, int workerId, int farmId)
    {
        return await _observationRepository.IsOwnerAsync(observationId, workerId, farmId);
    }
}