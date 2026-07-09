using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Application.Extensions;

namespace AgriculturePlatform.Application.Services;

public class ObservationService : IObservationService
{
    private readonly IObservationRepository _observationRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IFileStorageService _fileStorageService; // ✅ Added
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    // ✅ Updated constructor with IFileStorageService
    public ObservationService(
        IObservationRepository observationRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IFileStorageService fileStorageService, // ✅ Added
        INotificationService notificationService,
        IMapper mapper)
    {
        _observationRepository = observationRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _fileStorageService = fileStorageService; // ✅ Added
        _notificationService = notificationService;
        _mapper = mapper;
    }

    // =============================================
    // WORKER OPERATIONS
    // =============================================

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

        // ✅ Clean image paths
        var cleanedImagePath = CleanImagePath(dto.ImagePath);
        var cleanedAdditionalPaths = dto.AdditionalImagePaths?
            .Select(CleanImagePath)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList() ?? new List<string>();

        // ✅ Validate image paths exist
        if (!string.IsNullOrEmpty(cleanedImagePath))
        {
            var imageExists = await _fileStorageService.FileExistsAsync(cleanedImagePath);
            if (!imageExists)
            {
                return ApiResponse<ObservationDto>.Fail("Main image file not found. Please re-upload.");
            }
        }

        if (cleanedAdditionalPaths.Any())
        {
            foreach (var path in cleanedAdditionalPaths)
            {
                if (!await _fileStorageService.FileExistsAsync(path))
                    return ApiResponse<ObservationDto>.Fail($"Reference image '{path}' not found. Please re-upload.");
            }
        }

        var observation = new Observation
        {
            FarmId = farmId,
            WorkerId = workerId,
            AdminId = adminId,
            FieldId = dto.FieldId,
            CropCycleId = dto.CropCycleId,
            ObservationDate = dto.ObservationDate.ToUniversalTime(),
            PestType = dto.PestType,
            Notes = dto.Notes,
            ValidationStatus = "pending",
            ImagePath = cleanedImagePath,
            ThumbnailPath = CleanImagePath(dto.ThumbnailPath),
            ImageCaption = dto.ImageCaption,
            AdditionalImagePaths = cleanedAdditionalPaths,
            ImageMetadata = dto.ImageMetadata,
            IsImageVerified = false,
            CreatedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.CropHealth))
        {
            observation.CropHealth = Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
        }

        var created = await _observationRepository.CreateAsync(observation);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "Observation", created.Id, created, null, null);

        var workerName = (await _workerRepository.GetByIdAsync(workerId, farmId))?.Name ?? "A worker";
        await _notificationService.CreateAlertAggregateNotificationAsync(
            farmId,
            adminId,
            "Observations",
            "NewObservation",
            "/admin/observations",
            $"/admin/observations/{created.Id}"
        );

        var result = _mapper.Map<ObservationDto>(created);
        result.FieldName = field.FieldName;
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<ObservationDto>.Ok(result, "Observation created successfully");
    }

    public async Task<ApiResponse<ObservationDto>> UpdateOwnObservationAsync(int id, UpdateObservationDto dto, int workerId, int farmId)
    {
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<ObservationDto>.Fail("You don't have permission to update this observation");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }

        if (observation.ValidationStatus != "pending" && observation.ValidationStatus != "questioned")
        {
            return ApiResponse<ObservationDto>.Fail("Only pending or questioned observations can be edited.");
        }

        var oldObservation = _mapper.Map<Observation>(observation);

        // ✅ Update fields only if provided
        if (dto.FieldId.HasValue)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
                return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId.Value} not found");
            observation.FieldId = dto.FieldId.Value;
        }

        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId.Value} not found");
            observation.CropCycleId = dto.CropCycleId;
        }
        else if (dto.CropCycleId == null && dto.CropCycleId.HasValue == false)
        {
            observation.CropCycleId = null;
        }

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();

        if (!string.IsNullOrWhiteSpace(dto.CropHealth))
            observation.CropHealth = Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
        else if (dto.CropHealth != null)
            observation.CropHealth = null;

        if (dto.PestType != null)
            observation.PestType = string.IsNullOrWhiteSpace(dto.PestType) ? null : dto.PestType;

        if (dto.Notes != null)
            observation.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;

        if (dto.ImageCaption != null)
            observation.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;

        // ✅ Handle image updates
        if (dto.ImagePath != null)
            observation.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : CleanImagePath(dto.ImagePath);

        if (dto.ThumbnailPath != null)
            observation.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : CleanImagePath(dto.ThumbnailPath);

        if (dto.AdditionalImagePaths != null)
        {
            var cleanedPaths = dto.AdditionalImagePaths
                .Select(CleanImagePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            observation.AdditionalImagePaths = cleanedPaths.Any() ? cleanedPaths : new List<string>();
        }

        if (dto.ImageMetadata != null)
            observation.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = workerId;

        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Observation", observation.Id, oldObservation, observation, null, null);

        var result = _mapper.Map<ObservationDto>(observation);
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<ObservationDto>.Ok(result, "Observation updated successfully");
    }

    // ✅ PATCH: Partial update
    public async Task<ApiResponse<ObservationDto>> PatchObservationAsync(int id, UpdateObservationDto dto, int workerId, int farmId)
    {
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<ObservationDto>.Fail("You don't have permission to update this observation.");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");

        if (observation.ValidationStatus != "pending" && observation.ValidationStatus != "questioned")
        {
            return ApiResponse<ObservationDto>.Fail("Only pending or questioned observations can be edited.");
        }

        var oldObservation = _mapper.Map<Observation>(observation);
        var hasChanges = false;

        // ✅ Only update fields that are explicitly provided (not null)
        if (dto.FieldId.HasValue && dto.FieldId.Value != observation.FieldId)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
                return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId.Value} not found");
            observation.FieldId = dto.FieldId.Value;
            hasChanges = true;
        }

        if (dto.CropCycleId.HasValue && dto.CropCycleId.Value != observation.CropCycleId)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId.Value} not found");
            observation.CropCycleId = dto.CropCycleId;
            hasChanges = true;
        }

        if (dto.ObservationDate.HasValue && dto.ObservationDate.Value != observation.ObservationDate)
        {
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();
            hasChanges = true;
        }

        if (dto.CropHealth != null && dto.CropHealth != observation.CropHealth?.ToString())
        {
            observation.CropHealth = string.IsNullOrWhiteSpace(dto.CropHealth) ? null : Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
            hasChanges = true;
        }

        if (dto.PestType != null && dto.PestType != observation.PestType)
        {
            observation.PestType = string.IsNullOrWhiteSpace(dto.PestType) ? null : dto.PestType;
            hasChanges = true;
        }

        if (dto.Notes != null && dto.Notes != observation.Notes)
        {
            observation.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            hasChanges = true;
        }

        if (dto.ImageCaption != null && dto.ImageCaption != observation.ImageCaption)
        {
            observation.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;
            hasChanges = true;
        }

        // ✅ Handle image changes
        if (dto.ImagePath != null)
        {
            var newImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : CleanImagePath(dto.ImagePath);
            if (newImagePath != observation.ImagePath)
            {
                observation.ImagePath = newImagePath;
                hasChanges = true;
            }
        }

        if (dto.ThumbnailPath != null)
        {
            var newThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : CleanImagePath(dto.ThumbnailPath);
            if (newThumbnailPath != observation.ThumbnailPath)
            {
                observation.ThumbnailPath = newThumbnailPath;
                hasChanges = true;
            }
        }

        if (dto.AdditionalImagePaths != null)
        {
            var cleanedPaths = dto.AdditionalImagePaths
                .Select(CleanImagePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p!)
                .ToList();
            
            var newPaths = cleanedPaths.Any() ? cleanedPaths : new List<string>();
            
            var existingPaths = observation.AdditionalImagePaths ?? new List<string>();
            if (!existingPaths.SequenceEqual(newPaths))
            {
                observation.AdditionalImagePaths = newPaths;
                hasChanges = true;
            }
        }

        if (dto.ImageMetadata != null && dto.ImageMetadata != observation.ImageMetadata)
        {
            observation.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            var noChangeResult = _mapper.Map<ObservationDto>(observation);
            
            // ✅ Transform image URLs
            noChangeResult.WithPublicUrls(_fileStorageService);
            return ApiResponse<ObservationDto>.Ok(noChangeResult, "No changes detected");
        }

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = workerId;

        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Observation", observation.Id, oldObservation, observation, null, null);

        var result = _mapper.Map<ObservationDto>(observation);
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<ObservationDto>.Ok(result, "Observation updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteOwnObservationAsync(int id, int workerId, int farmId)
    {
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<bool>.Fail("You don't have permission to delete this observation");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<bool>.Fail($"Observation with ID {id} not found");
        }

        if (observation.ValidationStatus != "pending")
        {
            return ApiResponse<bool>.Fail("Only pending observations can be deleted.");
        }

        await CleanupObservationImagesAsync(observation);
        await _observationRepository.SoftDeleteAsync(observation, workerId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, null, "Observation", observation.Id, observation, null, null);

        return ApiResponse<bool>.Ok(true, "Observation deleted successfully");
    }

    // =============================================
    // WORKER RESPONSE OPERATIONS
    // =============================================

    public async Task<ApiResponse<ObservationDto>> RespondToAdminAsync(int id, ObservationWorkerResponseDto response, int farmId, int workerId)
    {
        if (!await _observationRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<ObservationDto>.Fail("You don't have permission to respond to this observation");
        }

        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }
        
        if (observation.ValidationStatus != "questioned")
        {
            return ApiResponse<ObservationDto>.Fail("Can only respond to observations that have been questioned by admin");
        }
        
        observation.WorkerResponse = response.WorkerResponse;
        observation.ValidationStatus = "pending";
        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = workerId;
        
        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Observation", observation.Id, 
            new { ValidationStatus = "questioned" }, 
            new { ValidationStatus = "pending" }, null, null);
        
        var workerName = (await _workerRepository.GetByIdAsync(workerId, farmId))?.Name ?? "A worker";
        await _notificationService.CreateAlertAggregateNotificationAsync(
            farmId,
            observation.AdminId,
            "Observation Updates",
            "ObservationUpdated",
            "/admin/observations",
            $"/admin/observations/{observation.Id}"
        );

        var result = _mapper.Map<ObservationDto>(observation);
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<ObservationDto>.Ok(result, "Response submitted, awaiting admin review");
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

        // Update fields
        if (dto.FieldId.HasValue)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
                return ApiResponse<ObservationDto>.Fail($"Field with ID {dto.FieldId.Value} not found");
            observation.FieldId = dto.FieldId.Value;
        }

        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
                return ApiResponse<ObservationDto>.Fail($"Crop cycle with ID {dto.CropCycleId.Value} not found");
            observation.CropCycleId = dto.CropCycleId;
        }
        else if (dto.CropCycleId == null && dto.CropCycleId.HasValue == false)
        {
            observation.CropCycleId = null;
        }

        if (dto.ObservationDate.HasValue)
            observation.ObservationDate = dto.ObservationDate.Value.ToUniversalTime();

        if (!string.IsNullOrWhiteSpace(dto.CropHealth))
            observation.CropHealth = Enum.Parse<CropHealthEnum>(dto.CropHealth, true);
        else if (dto.CropHealth != null)
            observation.CropHealth = null;

        if (dto.PestType != null)
            observation.PestType = string.IsNullOrWhiteSpace(dto.PestType) ? null : dto.PestType;

        if (dto.Notes != null)
            observation.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;

        if (dto.ImageCaption != null)
            observation.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;

        if (dto.ImagePath != null)
            observation.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : CleanImagePath(dto.ImagePath);

        if (dto.ThumbnailPath != null)
            observation.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : CleanImagePath(dto.ThumbnailPath);

        if (dto.AdditionalImagePaths != null)
        {
            var cleanedPaths = dto.AdditionalImagePaths
                .Select(CleanImagePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            observation.AdditionalImagePaths = cleanedPaths.Any() ? cleanedPaths : new List<string>();
        }

        if (dto.ImageMetadata != null)
            observation.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = adminId;

        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Observation", observation.Id, oldObservation, observation, null, null);

        var result = _mapper.Map<ObservationDto>(observation);
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<ObservationDto>.Ok(result, "Observation updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteObservationAsync(int id, int farmId, int adminId)
    {
        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<bool>.Fail($"Observation with ID {id} not found");
        }

        await CleanupObservationImagesAsync(observation);
        await _observationRepository.SoftDeleteAsync(observation, adminId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "Observation", observation.Id, observation, null, null);

        return ApiResponse<bool>.Ok(true, "Observation deleted successfully");
    }

    public async Task<ApiResponse<ObservationDto>> ValidateObservationAsync(int id, ObservationValidationDto validation, int farmId, int adminId)
    {
        var observation = await _observationRepository.GetByIdAsync(id, farmId);
        if (observation == null)
        {
            return ApiResponse<ObservationDto>.Fail($"Observation with ID {id} not found");
        }
        
        var oldStatus = observation.ValidationStatus;
        
        observation.ValidationStatus = validation.ValidationStatus;
        observation.AdminNotes = validation.AdminNotes;
        observation.FlagReason = validation.FlagReason;
        observation.ValidatedBy = adminId;
        observation.ValidatedAt = DateTime.UtcNow;
        observation.UpdatedAt = DateTime.UtcNow;
        observation.UpdatedBy = adminId;
        
        if (validation.ValidationStatus == "questioned")
        {
            observation.WorkerResponse = null;
        }
        
        await _observationRepository.UpdateAsync(observation);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Observation", observation.Id, 
            new { ValidationStatus = oldStatus }, 
            new { ValidationStatus = validation.ValidationStatus }, null, null);
        
        await _notificationService.CreateNotificationAsync(
            farmId,
            null,
            observation.WorkerId,
            "Observation Status Updated",
            $"Your observation was marked as {validation.ValidationStatus}.",
            "ObservationStatus",
            $"/worker/observations/{observation.Id}"
        );

        var result = _mapper.Map<ObservationDto>(observation);
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<ObservationDto>.Ok(result, $"Observation {validation.ValidationStatus}");
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

        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);

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
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<ObservationDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByFieldAsync(int fieldId, int farmId)
    {
        var observations = await _observationRepository.GetByFieldAsync(fieldId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        
        // ✅ Transform image URLs
        dtos.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByCropCycleAsync(int cropCycleId, int farmId)
    {
        var observations = await _observationRepository.GetByCropCycleAsync(cropCycleId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        
        // ✅ Transform image URLs
        dtos.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByWorkerAsync(int workerId, int farmId)
    {
        var observations = await _observationRepository.GetByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        
        // ✅ Transform image URLs
        dtos.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate)
    {
        var observations = await _observationRepository.GetByDateRangeAsync(farmId, fromDate.ToUniversalTime(), toDate.ToUniversalTime());
        var dtos = _mapper.Map<IEnumerable<ObservationDto>>(observations);
        
        // ✅ Transform image URLs
        dtos.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<IEnumerable<ObservationDto>>.Ok(dtos);
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
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
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
        
        // ✅ Transform image URLs
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<PagedResult<ObservationDto>>.Ok(result);
    }

    public async Task<ApiResponse<ObservationStatisticsDto>> GetPestStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _observationRepository.GetPestDetectionStatisticsAsync(farmId, fromDate?.ToUniversalTime(), toDate?.ToUniversalTime());
        
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
    // HELPER METHODS
    // =============================================

    private string? CleanImagePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            try
            {
                var uri = new Uri(path);
                path = uri.AbsolutePath.TrimStart('/');
            }
            catch
            {
                var parts = path.Split(new[] { "uploads/" }, StringSplitOptions.None);
                if (parts.Length > 1)
                    path = parts[parts.Length - 1];
            }
        }
        
        var cleaned = path.Replace("uploads/", "");
        cleaned = cleaned.TrimStart('/');
        
        return string.IsNullOrEmpty(cleaned) ? null : cleaned;
    }

    private async Task CleanupObservationImagesAsync(Observation observation)
    {
        try
        {
            if (!string.IsNullOrEmpty(observation.ImagePath))
                await _fileStorageService.DeleteFileAsync(observation.ImagePath);

            if (!string.IsNullOrEmpty(observation.ThumbnailPath))
                await _fileStorageService.DeleteFileAsync(observation.ThumbnailPath);

            if (observation.AdditionalImagePaths?.Any() == true)
            {
                foreach (var path in observation.AdditionalImagePaths)
                {
                    if (!string.IsNullOrEmpty(path))
                        await _fileStorageService.DeleteFileAsync(path);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up observation images: {ex.Message}");
        }
    }
}