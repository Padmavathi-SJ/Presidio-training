using System.Text.Json;
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Application.Extensions; // ✅ Using ImageUrlExtensions

namespace AgriculturePlatform.Application.Services;

public class HarvestService : IHarvestService
{
    private readonly IHarvestRepository _harvestRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public HarvestService(
        IHarvestRepository harvestRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _harvestRepository = harvestRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    // =============================================
    // WORKER OPERATIONS
    // =============================================

    public async Task<ApiResponse<HarvestDto>> CreateHarvestAsync(CreateHarvestDto dto, int farmId, int workerId, int adminId)
    {
        // Validate field access
        var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
        if (field == null)
            return ApiResponse<HarvestDto>.Fail($"Field with ID {dto.FieldId} not found");

        // Validate crop cycle
        var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId, farmId);
        if (cropCycle == null)
            return ApiResponse<HarvestDto>.Fail($"Crop cycle with ID {dto.CropCycleId} not found");

        // Clean image paths
        var cleanedImagePath = CleanImagePath(dto.ImagePath);
        var cleanedAdditionalPaths = dto.AdditionalImagePaths?
            .Select(CleanImagePath)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList() ?? new List<string>();

        // Validate image paths
        if (!string.IsNullOrEmpty(cleanedImagePath))
        {
            var imageExists = await _fileStorageService.FileExistsAsync(cleanedImagePath);
            if (!imageExists)
            {
                return ApiResponse<HarvestDto>.Fail("Main image file not found. Please re-upload.");
            }
        }

        if (cleanedAdditionalPaths.Any())
        {
            foreach (var path in cleanedAdditionalPaths)
            {
                if (!await _fileStorageService.FileExistsAsync(path))
                    return ApiResponse<HarvestDto>.Fail($"Reference image '{path}' not found. Please re-upload.");
            }
        }

        var harvest = new Harvest
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = dto.FieldId,
            CropCycleId = dto.CropCycleId,
            HarvestedBy = workerId,
            SubmittedBy = workerId,
            HarvestDate = dto.HarvestDate.ToUniversalTime(),
            QuantityKg = dto.QuantityKg,
            Notes = dto.Notes,
            PricePerKg = dto.PricePerKg,
            BatchNumber = dto.BatchNumber,
            ApprovalStatus = "PENDING",
            CreatedBy = workerId,
            CreatedAt = DateTime.UtcNow,
            ImagePath = cleanedImagePath,
            ThumbnailPath = CleanImagePath(dto.ThumbnailPath),
            ImageCaption = dto.ImageCaption,
            AdditionalImagePaths = cleanedAdditionalPaths,
            ImageMetadata = dto.ImageMetadata
        };

        if (!string.IsNullOrWhiteSpace(dto.QualityGrade))
        {
            harvest.QualityGrade = MapQualityGrade(dto.QualityGrade);
        }
        
        if (!string.IsNullOrWhiteSpace(dto.HarvestMethod))
        {
            harvest.HarvestMethod = Enum.Parse<HarvestMethodEnum>(dto.HarvestMethod, true);
        }

        var created = await _harvestRepository.CreateAsync(harvest);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "Harvest", created.Id, created, null, null);

        var workerName = (await _workerRepository.GetByIdAsync(workerId, farmId))?.Name ?? "A worker";
        await _notificationService.CreateAlertAggregateNotificationAsync(
            farmId,
            adminId,
            "Harvest Logs",
            "NewHarvest",
            "/admin/harvests",
            $"/admin/harvests/{created.Id}"
        );

        var result = _mapper.Map<HarvestDto>(created);
        result.FieldName = field.FieldName;
        
        // ✅ Using ImageUrlExtensions (works the same as HarvestExtensions)
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<HarvestDto>.Ok(result, "Harvest submitted for approval");
    }

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
                {
                    path = parts[parts.Length - 1];
                }
            }
        }
        
        var cleaned = path.Replace("uploads/", "");
        cleaned = cleaned.TrimStart('/');
        
        return string.IsNullOrEmpty(cleaned) ? null : cleaned;
    }

    private QualityGradeEnum? MapQualityGrade(string grade)
    {
        var gradeMap = new Dictionary<string, QualityGradeEnum>(StringComparer.OrdinalIgnoreCase)
        {
            { "A_PLUS", QualityGradeEnum.A_PLUS },
            { "PREMIUM", QualityGradeEnum.A_PLUS },
            { "A", QualityGradeEnum.A },
            { "GRADE_A", QualityGradeEnum.A },
            { "B", QualityGradeEnum.B },
            { "GRADE_B", QualityGradeEnum.B },
            { "C", QualityGradeEnum.C },
            { "GRADE_C", QualityGradeEnum.C },
            { "D", QualityGradeEnum.D },
            { "GRADE_D", QualityGradeEnum.D },
            { "REJECTED", QualityGradeEnum.REJECTED },
            { "REJECT", QualityGradeEnum.REJECTED }
        };
        
        return gradeMap.TryGetValue(grade, out var mappedGrade) ? mappedGrade : null;
    }

    public async Task<ApiResponse<HarvestDto>> UpdateOwnHarvestAsync(int id, UpdateHarvestDto dto, int workerId, int farmId)
    {
        if (!await _harvestRepository.CanWorkerEditAsync(id, workerId, farmId))
        {
            return ApiResponse<HarvestDto>.Fail("You don't have permission to update this harvest. Only pending or requested changes harvests can be edited.");
        }

        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var oldHarvest = _mapper.Map<Harvest>(harvest);

        if (dto.HarvestDate.HasValue)
            harvest.HarvestDate = dto.HarvestDate.Value.ToUniversalTime();
        
        if (dto.QuantityKg.HasValue)
            harvest.QuantityKg = dto.QuantityKg.Value;
        
        if (!string.IsNullOrWhiteSpace(dto.QualityGrade))
            harvest.QualityGrade = Enum.Parse<QualityGradeEnum>(dto.QualityGrade, true);
        
        if (!string.IsNullOrWhiteSpace(dto.HarvestMethod))
            harvest.HarvestMethod = Enum.Parse<HarvestMethodEnum>(dto.HarvestMethod, true);
        
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            harvest.Notes = dto.Notes;
        
        if (dto.PricePerKg.HasValue)
            harvest.PricePerKg = dto.PricePerKg;
        
        if (!string.IsNullOrWhiteSpace(dto.BatchNumber))
            harvest.BatchNumber = dto.BatchNumber;
        
        if (!string.IsNullOrWhiteSpace(dto.ImageCaption))
            harvest.ImageCaption = dto.ImageCaption;

        if (dto.ImagePath != null)
        {
            harvest.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : dto.ImagePath;
        }

        if (dto.AdditionalImagePaths != null)
        {
            var cleanedPaths = dto.AdditionalImagePaths
                .Select(CleanImagePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            
            harvest.AdditionalImagePaths = cleanedPaths.Any() ? cleanedPaths : new List<string>();
        }

        if (dto.ThumbnailPath != null)
            harvest.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : dto.ThumbnailPath;

        if (dto.ImageMetadata != null)
            harvest.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = workerId;
        
        if (harvest.ApprovalStatus == "REQUEST_CHANGES")
        {
            harvest.ApprovalStatus = "PENDING";
            harvest.WorkerResponse = null;
        }

        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Harvest", harvest.Id, oldHarvest, harvest, null, null);

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result, "Harvest updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteOwnHarvestAsync(int id, int workerId, int farmId)
    {
        if (!await _harvestRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<bool>.Fail("You don't have permission to delete this harvest");
        }

        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<bool>.Fail($"Harvest with ID {id} not found");

        if (harvest.ApprovalStatus == "APPROVED")
        {
            return ApiResponse<bool>.Fail("Cannot delete an approved harvest. Please contact an admin.");
        }

        await CleanupHarvestImagesAsync(harvest);
        await _harvestRepository.SoftDeleteAsync(harvest, workerId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, null, "Harvest", harvest.Id, harvest, null, null);

        return ApiResponse<bool>.Ok(true, "Harvest deleted successfully");
    }

    public async Task<ApiResponse<HarvestDto>> RespondToAdminAsync(int id, HarvestWorkerResponseDto response, int farmId, int workerId)
    {
        if (!await _harvestRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<HarvestDto>.Fail("You don't have permission to respond");
        }

        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");
        
        if (harvest.ApprovalStatus != "REQUEST_CHANGES")
        {
            return ApiResponse<HarvestDto>.Fail("Can only respond to harvests that need changes");
        }

        harvest.WorkerResponse = response.WorkerResponse;
        harvest.ApprovalStatus = "PENDING";
        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = workerId;
        
        await _harvestRepository.UpdateAsync(harvest);

        var workerName = (await _workerRepository.GetByIdAsync(workerId, farmId))?.Name ?? "A worker";
        await _notificationService.CreateAlertAggregateNotificationAsync(
            farmId,
            harvest.AdminId,
            "Harvest Updates",
            "HarvestUpdated",
            "/admin/harvests",
            $"/admin/harvests/{harvest.Id}"
        );

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result, "Response submitted");
    }

    // =============================================
    // ADMIN OPERATIONS
    // =============================================

    public async Task<ApiResponse<HarvestDto>> UpdateHarvestAsync(int id, UpdateHarvestDto dto, int farmId, int adminId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var oldHarvest = _mapper.Map<Harvest>(harvest);

        if (dto.HarvestDate.HasValue)
            harvest.HarvestDate = dto.HarvestDate.Value.ToUniversalTime();
        
        if (dto.QuantityKg.HasValue)
            harvest.QuantityKg = dto.QuantityKg.Value;
        
        if (!string.IsNullOrWhiteSpace(dto.QualityGrade))
            harvest.QualityGrade = Enum.Parse<QualityGradeEnum>(dto.QualityGrade, true);
        
        if (!string.IsNullOrWhiteSpace(dto.HarvestMethod))
            harvest.HarvestMethod = Enum.Parse<HarvestMethodEnum>(dto.HarvestMethod, true);
        
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            harvest.Notes = dto.Notes;
        
        if (dto.PricePerKg.HasValue)
            harvest.PricePerKg = dto.PricePerKg;
        
        if (!string.IsNullOrWhiteSpace(dto.BatchNumber))
            harvest.BatchNumber = dto.BatchNumber;
        
        if (!string.IsNullOrWhiteSpace(dto.ImageCaption))
            harvest.ImageCaption = dto.ImageCaption;

        if (dto.ImagePath != null)
        {
            harvest.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : dto.ImagePath;
        }

        if (dto.AdditionalImagePaths != null)
        {
            var cleanedPaths = dto.AdditionalImagePaths
                .Select(CleanImagePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            
            harvest.AdditionalImagePaths = cleanedPaths.Any() ? cleanedPaths : new List<string>();
        }

        if (dto.ThumbnailPath != null)
            harvest.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : dto.ThumbnailPath;

        if (dto.ImageMetadata != null)
            harvest.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = adminId;

        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Harvest", harvest.Id, oldHarvest, harvest, null, null);

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result, "Harvest updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteHarvestAsync(int id, int farmId, int adminId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<bool>.Fail($"Harvest with ID {id} not found");

        await CleanupHarvestImagesAsync(harvest);
        await _harvestRepository.SoftDeleteAsync(harvest, adminId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "Harvest", harvest.Id, harvest, null, null);

        return ApiResponse<bool>.Ok(true, "Harvest deleted successfully");
    }

    public async Task<ApiResponse<HarvestDto>> ApproveHarvestAsync(int id, HarvestApprovalDto approval, int farmId, int adminId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var oldStatus = harvest.ApprovalStatus;
        
        harvest.ApprovalStatus = approval.ApprovalStatus;
        harvest.ApprovedBy = adminId;
        harvest.ApprovedAt = DateTime.UtcNow;
        harvest.AdminNotes = approval.AdminNotes;
        
        if (approval.ApprovalStatus == "REJECTED")
        {
            harvest.RejectionReason = approval.RejectionReason;
        }
        
        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = adminId;
        
        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Harvest", harvest.Id, 
            new { ApprovalStatus = oldStatus }, 
            new { ApprovalStatus = approval.ApprovalStatus }, null, null);

        await _notificationService.CreateNotificationAsync(
            farmId,
            null,
            harvest.HarvestedBy,
            "Harvest Status Updated",
            $"Your harvest log was marked as {approval.ApprovalStatus}.",
            "HarvestStatus",
            $"/worker/harvests/{harvest.Id}"
        );

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result, $"Harvest {approval.ApprovalStatus.ToLower()}");
    }

    public async Task<ApiResponse<PagedResult<HarvestDto>>> GetAllHarvestsAsync(HarvestFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _harvestRepository.GetPagedAsync(
            farmId,
            filter.FieldId,
            filter.CropCycleId,
            filter.WorkerId,
            filter.ApprovalStatus,
            filter.QualityGrade,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<HarvestDto>>(pagedResult.Items);
        
        var result = new PagedResult<HarvestDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);

        return ApiResponse<PagedResult<HarvestDto>>.Ok(result);
    }

    public async Task<ApiResponse<HarvestDto>> GetHarvestByIdAsync(int id, int farmId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByFieldAsync(int fieldId, int farmId)
    {
        var harvests = await _harvestRepository.GetByFieldAsync(fieldId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
        
        // ✅ Using ImageUrlExtensions
        dtos.WithPublicUrls(_fileStorageService);
        return ApiResponse<IEnumerable<HarvestDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByCropCycleAsync(int cropCycleId, int farmId)
    {
        var harvests = await _harvestRepository.GetByCropCycleAsync(cropCycleId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
        
        // ✅ Using ImageUrlExtensions
        dtos.WithPublicUrls(_fileStorageService);
        return ApiResponse<IEnumerable<HarvestDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByWorkerAsync(int workerId, int farmId)
    {
        var harvests = await _harvestRepository.GetByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
        
        // ✅ Using ImageUrlExtensions
        dtos.WithPublicUrls(_fileStorageService);
        return ApiResponse<IEnumerable<HarvestDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PagedResult<HarvestDto>>> GetPendingApprovalsAsync(int farmId, PaginationParams pagination)
    {
        var harvests = await _harvestRepository.GetPendingApprovalsAsync(farmId);
        
        var paged = harvests
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();
        
        var dtos = _mapper.Map<List<HarvestDto>>(paged);
        
        var result = new PagedResult<HarvestDto>
        {
            Items = dtos,
            TotalCount = harvests.Count(),
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        
        return ApiResponse<PagedResult<HarvestDto>>.Ok(result);
    }

    // =============================================
    // STATISTICS
    // =============================================

    public async Task<ApiResponse<YieldStatisticsDto>> GetYieldStatisticsAsync(int farmId, int? cropCycleId, DateTime? fromDate, DateTime? toDate)
    {
        var stats = await _harvestRepository.GetYieldStatisticsAsync(farmId, cropCycleId, fromDate, toDate);
        return ApiResponse<YieldStatisticsDto>.Ok(stats);
    }

    public async Task<ApiResponse<YieldStatisticsDto>> GetYearOverYearComparisonAsync(int farmId, int currentYear, int? previousYear)
    {
        var prevYear = previousYear ?? currentYear - 1;
        
        var currentStats = await _harvestRepository.GetYieldStatisticsAsync(farmId, null, 
            new DateTime(currentYear, 1, 1), new DateTime(currentYear, 12, 31));
        
        var previousStats = await _harvestRepository.GetYieldStatisticsAsync(farmId, null,
            new DateTime(prevYear, 1, 1), new DateTime(prevYear, 12, 31));
        
        currentStats.PreviousSeasonYield = previousStats.TotalYieldKg;
        currentStats.YieldGrowthPercentage = previousStats.TotalYieldKg > 0 
            ? Math.Round(((currentStats.TotalYieldKg - previousStats.TotalYieldKg) / previousStats.TotalYieldKg) * 100, 2)
            : 0;
        
        return ApiResponse<YieldStatisticsDto>.Ok(currentStats);
    }

    // =============================================
    // VALIDATION
    // =============================================

    public async Task<bool> ValidateHarvestOwnershipAsync(int harvestId, int workerId, int farmId)
    {
        return await _harvestRepository.IsOwnerAsync(harvestId, workerId, farmId);
    }

    public async Task<bool> HasPendingApprovalsAsync(int workerId, int farmId)
    {
        return await _harvestRepository.HasPendingApprovalAsync(workerId, farmId);
    }

    // =============================================
    // PATCH OPERATION
    // =============================================

    public async Task<ApiResponse<HarvestDto>> PatchHarvestAsync(int id, UpdateHarvestDto dto, int workerId, int farmId)
    {
        if (!await _harvestRepository.CanWorkerEditAsync(id, workerId, farmId))
        {
            return ApiResponse<HarvestDto>.Fail("You don't have permission to update this harvest. Only pending or requested changes harvests can be edited.");
        }

        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var oldHarvest = _mapper.Map<Harvest>(harvest);
        var hasChanges = false;

        if (dto.HarvestDate.HasValue && dto.HarvestDate.Value != harvest.HarvestDate)
        {
            harvest.HarvestDate = dto.HarvestDate.Value.ToUniversalTime();
            hasChanges = true;
        }

        if (dto.QuantityKg.HasValue && dto.QuantityKg.Value != harvest.QuantityKg)
        {
            harvest.QuantityKg = dto.QuantityKg.Value;
            hasChanges = true;
        }

        if (dto.QualityGrade != null && dto.QualityGrade != harvest.QualityGrade?.ToString())
        {
            harvest.QualityGrade = string.IsNullOrWhiteSpace(dto.QualityGrade) ? null : MapQualityGrade(dto.QualityGrade);
            hasChanges = true;
        }

        if (dto.HarvestMethod != null && dto.HarvestMethod != harvest.HarvestMethod?.ToString())
        {
            harvest.HarvestMethod = string.IsNullOrWhiteSpace(dto.HarvestMethod) ? null : Enum.Parse<HarvestMethodEnum>(dto.HarvestMethod, true);
            hasChanges = true;
        }

        if (dto.Notes != null && dto.Notes != harvest.Notes)
        {
            harvest.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            hasChanges = true;
        }

        if (dto.PricePerKg.HasValue && dto.PricePerKg.Value != harvest.PricePerKg)
        {
            harvest.PricePerKg = dto.PricePerKg.Value;
            hasChanges = true;
        }

        if (dto.BatchNumber != null && dto.BatchNumber != harvest.BatchNumber)
        {
            harvest.BatchNumber = string.IsNullOrWhiteSpace(dto.BatchNumber) ? null : dto.BatchNumber;
            hasChanges = true;
        }

        if (dto.ImageCaption != null && dto.ImageCaption != harvest.ImageCaption)
        {
            harvest.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;
            hasChanges = true;
        }

        if (dto.ImagePath != null)
        {
            var newImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : CleanImagePath(dto.ImagePath);
            if (newImagePath != harvest.ImagePath)
            {
                harvest.ImagePath = newImagePath;
                hasChanges = true;
            }
        }

        if (dto.ThumbnailPath != null)
        {
            var newThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : CleanImagePath(dto.ThumbnailPath);
            if (newThumbnailPath != harvest.ThumbnailPath)
            {
                harvest.ThumbnailPath = newThumbnailPath;
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
            
            var existingPaths = harvest.AdditionalImagePaths ?? new List<string>();
            if (!existingPaths.SequenceEqual(newPaths))
            {
                harvest.AdditionalImagePaths = newPaths;
                hasChanges = true;
            }
        }

        if (dto.ImageMetadata != null && dto.ImageMetadata != harvest.ImageMetadata)
        {
            harvest.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            var noChangeResult = _mapper.Map<HarvestDto>(harvest);
            
            // ✅ Using ImageUrlExtensions
            noChangeResult.WithPublicUrls(_fileStorageService);
            return ApiResponse<HarvestDto>.Ok(noChangeResult, "No changes detected");
        }

        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = workerId;
        
        if (harvest.ApprovalStatus == "REQUEST_CHANGES")
        {
            harvest.ApprovalStatus = "PENDING";
            harvest.WorkerResponse = null;
        }

        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Harvest", harvest.Id, oldHarvest, harvest, null, null);

        var result = _mapper.Map<HarvestDto>(harvest);
        
        // ✅ Using ImageUrlExtensions
        result.WithPublicUrls(_fileStorageService);
        return ApiResponse<HarvestDto>.Ok(result, "Harvest updated successfully");
    }

    // =============================================
    // IMAGE CLEANUP HELPER
    // =============================================

    private async Task CleanupHarvestImagesAsync(Harvest harvest)
    {
        try
        {
            if (!string.IsNullOrEmpty(harvest.ImagePath))
            {
                await _fileStorageService.DeleteFileAsync(harvest.ImagePath);
            }

            if (!string.IsNullOrEmpty(harvest.ThumbnailPath))
            {
                await _fileStorageService.DeleteFileAsync(harvest.ThumbnailPath);
            }

            if (harvest.AdditionalImagePaths?.Any() == true)
            {
                foreach (var path in harvest.AdditionalImagePaths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        await _fileStorageService.DeleteFileAsync(path);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up harvest images: {ex.Message}");
        }
    }
}