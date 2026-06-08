// AgriculturePlatform.Application/Services/CropCycleService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class CropCycleService : ICropCycleService
{
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public CropCycleService(
        ICropCycleRepository cropCycleRepository,
        IFieldRepository fieldRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _cropCycleRepository = cropCycleRepository;
        _fieldRepository = fieldRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

public async Task<ApiResponse<CropCycleDto>> CreateAsync(CreateCropCycleDto dto, int farmId, int adminId, string ipAddress, string userAgent)
{
    // Validate field exists and belongs to farm
    var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
    if (field == null)
    {
        return ApiResponse<CropCycleDto>.Fail($"Field with ID {dto.FieldId} not found");
    }

    // Parse enums
    var cropType = Enum.Parse<CropTypeEnum>(dto.CropType, true);
    var growthStage = Enum.Parse<GrowthStageEnum>(dto.GrowthStage ?? "GERMINATION", true);
    
    // Convert status string to TaskStatusEnum
    TaskStatusEnum status = TaskStatusEnum.IN_PROGRESS; // Default for active
if (!string.IsNullOrWhiteSpace(dto.Status))
{
    status = dto.Status.ToUpper() switch
    {
        "ACTIVE" or "IN_PROGRESS" => TaskStatusEnum.IN_PROGRESS,
        "COMPLETED" or "HARVESTED" => TaskStatusEnum.COMPLETED,
        "CANCELLED" or "FAILED" => TaskStatusEnum.CANCELLED,
        "PENDING" => TaskStatusEnum.PENDING,
        _ => TaskStatusEnum.IN_PROGRESS
    };
}

    // Create crop cycle
    var cropCycle = new CropCycle
    {
        FarmId = farmId,           // ← ADD THIS - Foreign key to Farms table
        AdminId = adminId,
        FieldId = dto.FieldId,
        CropType = cropType,
        PlantingDate = dto.PlantingDate,
        ExpectedHarvestDate = dto.ExpectedHarvestDate,
        GrowthStage = growthStage,
        Status = status,  //  Now assigning enum
        CreatedBy = adminId,
        CreatedAt = DateTime.UtcNow
    };

    var created = await _cropCycleRepository.CreateAsync(cropCycle);

    // Audit log
    await _auditLogService.LogCreateAsync(farmId, adminId, "CropCycle", created.Id, created, ipAddress, userAgent);

    var result = _mapper.Map<CropCycleDto>(created);
    return ApiResponse<CropCycleDto>.Ok(result, "Crop cycle created successfully");
}
    public async Task<ApiResponse<CropCycleDto>> UpdateAsync(int id, UpdateCropCycleDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var cropCycle = await _cropCycleRepository.GetByIdAsync(id, farmId);
        if (cropCycle == null)
        {
            return ApiResponse<CropCycleDto>.Fail($"Crop cycle with ID {id} not found");
        }

        var oldCropCycle = _mapper.Map<CropCycle>(cropCycle);

        // Update properties
        if (!string.IsNullOrWhiteSpace(dto.CropType))
        {
            cropCycle.CropType = Enum.Parse<CropTypeEnum>(dto.CropType, true);
        }
        if (dto.PlantingDate.HasValue)
        {
            cropCycle.PlantingDate = dto.PlantingDate;
        }
        if (dto.ExpectedHarvestDate.HasValue)
        {
            cropCycle.ExpectedHarvestDate = dto.ExpectedHarvestDate;
        }
        if (!string.IsNullOrWhiteSpace(dto.GrowthStage))
        {
            cropCycle.GrowthStage = Enum.Parse<GrowthStageEnum>(dto.GrowthStage, true);
        }
       // FIX: Convert string status to TaskStatusEnum
    if (!string.IsNullOrWhiteSpace(dto.Status))
    {
        cropCycle.Status = dto.Status.ToUpper() switch
        {
            "ACTIVE" => TaskStatusEnum.IN_PROGRESS,
            "IN_PROGRESS" => TaskStatusEnum.IN_PROGRESS,
            "COMPLETED" => TaskStatusEnum.COMPLETED,
            "HARVESTED" => TaskStatusEnum.COMPLETED,
            "CANCELLED" => TaskStatusEnum.CANCELLED,
            "FAILED" => TaskStatusEnum.CANCELLED,
            _ => TaskStatusEnum.PENDING
        };
    }

        cropCycle.UpdatedAt = DateTime.UtcNow;
        cropCycle.UpdatedBy = adminId;

        await _cropCycleRepository.UpdateAsync(cropCycle);

        // Audit log
        await _auditLogService.LogUpdateAsync(farmId, adminId, "CropCycle", cropCycle.Id, oldCropCycle, cropCycle, ipAddress, userAgent);

        var result = _mapper.Map<CropCycleDto>(cropCycle);
        return ApiResponse<CropCycleDto>.Ok(result, "Crop cycle updated successfully");
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var cropCycle = await _cropCycleRepository.GetByIdAsync(id, farmId, true);
        if (cropCycle == null)
        {
            return ApiResponse<bool>.Fail($"Crop cycle with ID {id} not found");
        }

        if (cropCycle.IsDeleted)
        {
            return ApiResponse<bool>.Fail($"Crop cycle with ID {id} is already deleted");
        }

        await _cropCycleRepository.SoftDeleteAsync(cropCycle, adminId);

        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "CropCycle", cropCycle.Id, cropCycle, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Crop cycle deleted successfully");
    }

    public async Task<ApiResponse<CropCycleDto>> GetByIdAsync(int id, int farmId)
    {
        var cropCycle = await _cropCycleRepository.GetByIdAsync(id, farmId);
        if (cropCycle == null)
        {
            return ApiResponse<CropCycleDto>.Fail($"Crop cycle with ID {id} not found");
        }

        var result = _mapper.Map<CropCycleDto>(cropCycle);
        return ApiResponse<CropCycleDto>.Ok(result);
    }

    public async Task<ApiResponse<PagedResult<CropCycleDto>>> GetAllAsync(CropCycleFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _cropCycleRepository.GetPagedAsync(
            farmId,
            filter.FieldId,
            filter.CropType,
            filter.GrowthStage,
            filter.Status,
            filter.ExpectedHarvestDateFrom,
            filter.ExpectedHarvestDateTo,
            filter.ActiveOnly,
            filter.OverdueOnly,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<CropCycleDto>>(pagedResult.Items);
        var result = new PagedResult<CropCycleDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<CropCycleDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<CropCycleDto>>> GetOverdueAsync(int farmId)
    {
        var overdueCycles = await _cropCycleRepository.GetOverdueCropCyclesAsync(farmId);
        var result = _mapper.Map<IEnumerable<CropCycleDto>>(overdueCycles);
        return ApiResponse<IEnumerable<CropCycleDto>>.Ok(result);
    }

    public async Task<bool> ValidateCropCycleOwnershipAsync(int cropCycleId, int farmId)
    {
        return await _cropCycleRepository.ExistsAsync(cropCycleId, farmId);
    }
}