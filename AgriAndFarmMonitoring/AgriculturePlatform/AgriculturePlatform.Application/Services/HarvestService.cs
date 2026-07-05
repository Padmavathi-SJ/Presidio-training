// Application/Services/HarvestService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class HarvestService : IHarvestService
{
    private readonly IHarvestRepository _harvestRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public HarvestService(
        IHarvestRepository harvestRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _harvestRepository = harvestRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
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
            // Image fields
            ImagePath = dto.ImagePath,
            ThumbnailPath = dto.ThumbnailPath,
            ImageCaption = dto.ImageCaption,
            AdditionalImagePaths = dto.AdditionalImagePaths ?? new List<string>(),
            ImageMetadata = dto.ImageMetadata
        };

        if (!string.IsNullOrWhiteSpace(dto.QualityGrade))
            harvest.QualityGrade = Enum.Parse<QualityGradeEnum>(dto.QualityGrade, true);
        
        if (!string.IsNullOrWhiteSpace(dto.HarvestMethod))
            harvest.HarvestMethod = Enum.Parse<HarvestMethodEnum>(dto.HarvestMethod, true);

        var created = await _harvestRepository.CreateAsync(harvest);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "Harvest", created.Id, created, null, null);

        var result = _mapper.Map<HarvestDto>(created);
        result.FieldName = field.FieldName;
        
        return ApiResponse<HarvestDto>.Ok(result, "Harvest submitted for approval");
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
        // Image fields
        if (dto.ImagePath != null)
            harvest.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : dto.ImagePath;
        if (dto.ThumbnailPath != null)
            harvest.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : dto.ThumbnailPath;
        if (dto.ImageCaption != null)
            harvest.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;
        if (dto.AdditionalImagePaths != null)
            harvest.AdditionalImagePaths = dto.AdditionalImagePaths;
        if (dto.ImageMetadata != null)
            harvest.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = workerId;
        
        // Reset status to PENDING if it was REQUEST_CHANGES
        if (harvest.ApprovalStatus == "REQUEST_CHANGES")
        {
            harvest.ApprovalStatus = "PENDING";
            harvest.WorkerResponse = null;
        }

        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "Harvest", harvest.Id, oldHarvest, harvest, null, null);

        var result = _mapper.Map<HarvestDto>(harvest);
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

        // Only allow deletion of PENDING or REJECTED harvests
        if (harvest.ApprovalStatus == "APPROVED")
        {
            return ApiResponse<bool>.Fail("Cannot delete an approved harvest. Please contact an admin.");
        }

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

        var result = _mapper.Map<HarvestDto>(harvest);
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
        // Image fields
        if (dto.ImagePath != null)
            harvest.ImagePath = string.IsNullOrWhiteSpace(dto.ImagePath) ? null : dto.ImagePath;
        if (dto.ThumbnailPath != null)
            harvest.ThumbnailPath = string.IsNullOrWhiteSpace(dto.ThumbnailPath) ? null : dto.ThumbnailPath;
        if (dto.ImageCaption != null)
            harvest.ImageCaption = string.IsNullOrWhiteSpace(dto.ImageCaption) ? null : dto.ImageCaption;
        if (dto.AdditionalImagePaths != null)
            harvest.AdditionalImagePaths = dto.AdditionalImagePaths;
        if (dto.ImageMetadata != null)
            harvest.ImageMetadata = string.IsNullOrWhiteSpace(dto.ImageMetadata) ? null : dto.ImageMetadata;

        harvest.UpdatedAt = DateTime.UtcNow;
        harvest.UpdatedBy = adminId;

        await _harvestRepository.UpdateAsync(harvest);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Harvest", harvest.Id, oldHarvest, harvest, null, null);

        var result = _mapper.Map<HarvestDto>(harvest);
        return ApiResponse<HarvestDto>.Ok(result, "Harvest updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteHarvestAsync(int id, int farmId, int adminId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<bool>.Fail($"Harvest with ID {id} not found");

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

        var result = _mapper.Map<HarvestDto>(harvest);
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

        return ApiResponse<PagedResult<HarvestDto>>.Ok(result);
    }

    public async Task<ApiResponse<HarvestDto>> GetHarvestByIdAsync(int id, int farmId)
    {
        var harvest = await _harvestRepository.GetByIdAsync(id, farmId);
        if (harvest == null)
            return ApiResponse<HarvestDto>.Fail($"Harvest with ID {id} not found");

        var result = _mapper.Map<HarvestDto>(harvest);
        return ApiResponse<HarvestDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByFieldAsync(int fieldId, int farmId)
    {
        var harvests = await _harvestRepository.GetByFieldAsync(fieldId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
        return ApiResponse<IEnumerable<HarvestDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByCropCycleAsync(int cropCycleId, int farmId)
    {
        var harvests = await _harvestRepository.GetByCropCycleAsync(cropCycleId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
        return ApiResponse<IEnumerable<HarvestDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByWorkerAsync(int workerId, int farmId)
    {
        var harvests = await _harvestRepository.GetByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<HarvestDto>>(harvests);
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
}