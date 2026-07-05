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

// AgriculturePlatform.Application/Services/ObservationService.cs

public async Task<ApiResponse<ObservationDto>> CreateObservationAsync(CreateObservationDto dto, int farmId, int workerId, int adminId)
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
        WorkerId = workerId,
        FieldId = dto.FieldId,
        CropCycleId = dto.CropCycleId,
        ObservationDate = dto.ObservationDate.ToUniversalTime(),
        PestType = dto.PestType,
        Notes = dto.Notes,
        // Images
        ImagePath = dto.ImagePath,
        ThumbnailPath = dto.ThumbnailPath,
        ImageCaption = dto.ImageCaption,
        AdditionalImagePaths = dto.AdditionalImagePaths,
        ImageMetadata = dto.ImageMetadata
    };
    observation.AdminId = adminId;
    observation.CropHealth = !string.IsNullOrWhiteSpace(dto.CropHealth) 
        ? Enum.Parse<CropHealthEnum>(dto.CropHealth, true) 
        : null;
    observation.CreatedBy = workerId;
    observation.CreatedAt = DateTime.UtcNow;

    var created = await _observationRepository.CreateAsync(observation);
    
    await _auditLogService.LogCreateAsync(
        farmId,           
        adminId,          // ← AdminId from worker
        "Observation",    
        created.Id,       
        created,          
        null,             
        null);            

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

        // Update Field association if provided
        if (dto.FieldId.HasValue)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
                return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId.Value} not found");
            observation.FieldId = dto.FieldId.Value;
        }

        // Update Crop Cycle association if provided
        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId.Value} not found");
            observation.CropCycleId = dto.CropCycleId;
        }
        else
        {
            observation.CropCycleId = null;
        }

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();

        observation.CropHealth = !string.IsNullOrWhiteSpace(dto.CropHealth)
            ? Enum.Parse<CropHealthEnum>(dto.CropHealth, true)
            : null;

        observation.PestType = dto.PestType;
        observation.Notes = dto.Notes;

        // Update image fields
        observation.ImagePath = dto.ImagePath;
        observation.ThumbnailPath = dto.ThumbnailPath;
        observation.ImageCaption = dto.ImageCaption;
        observation.AdditionalImagePaths = dto.AdditionalImagePaths;
        observation.ImageMetadata = dto.ImageMetadata;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = workerId;

        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(
            farmId,           // farmId
            null,             // adminId (null for worker)
            "Observation",    // entityType
            observation.Id,   // entityId
            oldObservation,   // oldEntity
            observation,      // newEntity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        // Resolve Field Name dynamically
        var updatedField = await _fieldRepository.GetByIdAsync(observation.FieldId, farmId);
        var result = _mapper.Map<ObservationDto>(observation);
        if (updatedField != null)
        {
            result.FieldName = updatedField.FieldName;
        }

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

        // Update Field association if provided
        if (dto.FieldId.HasValue)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
                return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId.Value} not found");
            observation.FieldId = dto.FieldId.Value;
        }

        // Update Crop Cycle association if provided
        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId.Value} not found");
            observation.CropCycleId = dto.CropCycleId;
        }
        else
        {
            observation.CropCycleId = null;
        }

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();

        observation.CropHealth = !string.IsNullOrWhiteSpace(dto.CropHealth)
            ? Enum.Parse<CropHealthEnum>(dto.CropHealth, true)
            : null;

        observation.PestType = dto.PestType;
        observation.Notes = dto.Notes;

        // Update image fields
        observation.ImagePath = dto.ImagePath;
        observation.ThumbnailPath = dto.ThumbnailPath;
        observation.ImageCaption = dto.ImageCaption;
        observation.AdditionalImagePaths = dto.AdditionalImagePaths;
        observation.ImageMetadata = dto.ImageMetadata;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = adminId;

        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(
            farmId,           // farmId
            adminId,          // adminId
            "Observation",    // entityType
            observation.Id,   // entityId
            oldObservation,   // oldEntity
            observation,      // newEntity
            null,             // ipAddress (optional)
            null);            // userAgent (optional)

        // Resolve Field Name dynamically
        var updatedField = await _fieldRepository.GetByIdAsync(observation.FieldId, farmId);
        var result = _mapper.Map<ObservationDto>(observation);
        if (updatedField != null)
        {
            result.FieldName = updatedField.FieldName;
        }

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
            filter.FromDate?.ToUniversalTime(),
            filter.ToDate?.ToUniversalTime(),
            filter.ValidationStatus,
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




// =============================================
// WORKER RESPONSE OPERATIONS
// =============================================

public async Task<ApiResponse<ObservationDto>> RespondToAdminAsync(int id, ObservationWorkerResponseDto response, int farmId, int workerId)
{
    // Verify ownership
    if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
    {
        return ApiResponse<ObservationDto>.Fail("You don't have permission to respond to this observation");
    }

    var observation = await _observationRepository.GetByIdAsync(id, farmId);
    if (observation == null)
    {
        return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
    }
    
    // Only allow response if status is "questioned"
    if (observation.ValidationStatus != "questioned")
    {
        return ApiResponse<ObservationDto>.Fail("Can only respond to observations that have been questioned by admin");
    }
    
    var oldStatus = observation.ValidationStatus;
    var oldResponse = observation.WorkerResponse;
    
    observation.WorkerResponse = response.WorkerResponse;
    observation.ValidationStatus = "pending";  // Back to pending for re-review
    observation.UpdatedAt = DateTime.UtcNow;
    observation.UpdatedBy = workerId;
    
    await _observationRepository.UpdateAsync(observation);
    
    // Log the response
    await _auditLogService.LogUpdateAsync(
        farmId, null, "Observation", observation.Id,
        new { ValidationStatus = oldStatus, WorkerResponse = oldResponse },
        new { ValidationStatus = "pending", WorkerResponse = response.WorkerResponse });
    
    var result = _mapper.Map<ObservationDto>(observation);
    return ApiResponse<ObservationDto>.Ok(result, "Response submitted, awaiting admin review");
}

// =============================================
// ADMIN VALIDATION OPERATIONS
// =============================================

public async Task<ApiResponse<ObservationDto>> ValidateObservationAsync(int id, ObservationValidationDto validation, int farmId, int adminId)
{
    var observation = await _observationRepository.GetByIdAsync(id, farmId);
    if (observation == null)
    {
        return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
    }
    
    var oldStatus = observation.ValidationStatus;
    var oldNotes = observation.AdminNotes;
    
    observation.ValidationStatus = validation.ValidationStatus;
    observation.AdminNotes = validation.AdminNotes;
    observation.FlagReason = validation.FlagReason;
    observation.ValidatedBy = adminId;
    observation.ValidatedAt = DateTime.UtcNow;
    observation.UpdatedAt = DateTime.UtcNow;
    observation.UpdatedBy = adminId;
    
    // If marking as questioned, clear any previous response
    if (validation.ValidationStatus == "questioned")
    {
        observation.WorkerResponse = null;
    }
    
    await _observationRepository.UpdateAsync(observation);
    
    // Log validation
    await _auditLogService.LogUpdateAsync(
        farmId, adminId, "Observation", observation.Id,
        new { ValidationStatus = oldStatus, AdminNotes = oldNotes },
        new { ValidationStatus = validation.ValidationStatus, AdminNotes = validation.AdminNotes });
    
    // Create notification for worker if questioned
    if (validation.ValidationStatus == "questioned" && observation.WorkerId.HasValue)
    {
        // TODO: Call notification service
        // await _notificationService.CreateNotificationAsync(...)
    }
    
    var result = _mapper.Map<ObservationDto>(observation);
    return ApiResponse<ObservationDto>.Ok(result, $"Observation {validation.ValidationStatus}");
}

public async Task<ApiResponse<PagedResult<ObservationDto>>> GetPendingValidationsAsync(int farmId, PaginationParams pagination)
{
    var observations = await _observationRepository.GetPendingValidationsAsync(farmId);
    
    var paged = observations
        .Skip((pagination.Page - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToList();
    
    var dtos = _mapper.Map<List<ObservationDto>>(paged);
    
    var result = new PagedResult<ObservationDto>
    {
        Items = dtos,
        TotalCount = observations.Count(),
        Page = pagination.Page,
        PageSize = pagination.PageSize
    };
    
    return ApiResponse<PagedResult<ObservationDto>>.Ok(result);
}

public async Task<ApiResponse<PagedResult<ObservationDto>>> GetQuestionedObservationsAsync(int farmId, PaginationParams pagination)
{
    var observations = await _observationRepository.GetQuestionedObservationsAsync(farmId);
    
    var paged = observations
        .Skip((pagination.Page - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToList();
    
    var dtos = _mapper.Map<List<ObservationDto>>(paged);
    
    var result = new PagedResult<ObservationDto>
    {
        Items = dtos,
        TotalCount = observations.Count(),
        Page = pagination.Page,
        PageSize = pagination.PageSize
    };
    
    return ApiResponse<PagedResult<ObservationDto>>.Ok(result);
}



}



