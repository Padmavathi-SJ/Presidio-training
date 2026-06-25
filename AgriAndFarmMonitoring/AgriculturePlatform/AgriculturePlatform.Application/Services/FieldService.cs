using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Application.Validators;

namespace AgriculturePlatform.Application.Services;

public class FieldService : IFieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IExcelService _excelService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public FieldService(
        IFieldRepository fieldRepository,
        IExcelService excelService,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _fieldRepository = fieldRepository;
        _excelService = excelService;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

// Update the CreateAsync method in FieldService.cs

public async Task<ApiResponse<FieldDto>> CreateAsync(CreateFieldDto dto, int farmId, int adminId, string ipAddress, string userAgent)
{
    // Validate adminId
    if (adminId <= 0)
    {
        return ApiResponse<FieldDto>.Fail("Invalid admin ID. Please login again.");
    }

    // Validate coordinates if provided
    if (dto.Latitude.HasValue && (dto.Latitude < -90 || dto.Latitude > 90))
    {
        return ApiResponse<FieldDto>.Fail("Latitude must be between -90 and 90 degrees");
    }
    if (dto.Longitude.HasValue && (dto.Longitude < -180 || dto.Longitude > 180))
    {
        return ApiResponse<FieldDto>.Fail("Longitude must be between -180 and 180 degrees");
    }
    if ((dto.Latitude.HasValue && !dto.Longitude.HasValue) || (!dto.Latitude.HasValue && dto.Longitude.HasValue))
    {
        return ApiResponse<FieldDto>.Fail("Both Latitude and Longitude must be provided together");
    }

    // Check if field name already exists
    if (await _fieldRepository.FieldNameExistsAsync(dto.FieldName, farmId))
    {
        return ApiResponse<FieldDto>.Fail($"Field with name '{dto.FieldName}' already exists");
    }

    // Create field entity
    var field = new Field
    {
        FarmId = farmId,
        AdminId = adminId,
        CreatedBy = adminId,
        FieldName = dto.FieldName,
        Location = dto.Location,
        AreaHectares = dto.AreaHectares,
        SoilType = Enum.TryParse<SoilTypeEnum>(dto.SoilType, true, out var soilType) ? soilType : null,
        Status = Enum.TryParse<FieldStatusEnum>(dto.Status, true, out var status) ? status : FieldStatusEnum.ACTIVE,
        Latitude = dto.Latitude,      // ADD THIS
        Longitude = dto.Longitude,    // ADD THIS
        CreatedAt = DateTime.UtcNow
    };

    var created = await _fieldRepository.CreateAsync(field);
    
    // Audit log
    await _auditLogService.LogCreateAsync(farmId, adminId, "Field", created.Id, created, ipAddress, userAgent);
    
    var result = _mapper.Map<FieldDto>(created);
    result.ActiveCropCount = await _fieldRepository.GetActiveCropsCountAsync(created.Id);
    
    return ApiResponse<FieldDto>.Ok(result, "Field created successfully");
}
// In FieldService.cs, modify the UpdateAsync method

// Update the UpdateAsync method in FieldService.cs

public async Task<ApiResponse<FieldDto>> UpdateAsync(int id, UpdateFieldDto dto, int farmId, int adminId, string ipAddress, string userAgent)
{
    var field = await _fieldRepository.GetByIdAsync(id, farmId);
    if (field == null)
    {
        return ApiResponse<FieldDto>.Fail($"Field with ID {id} not found");
    }

    // Validate coordinates if provided
    if (dto.Latitude.HasValue && (dto.Latitude < -90 || dto.Latitude > 90))
    {
        return ApiResponse<FieldDto>.Fail("Latitude must be between -90 and 90 degrees");
    }
    if (dto.Longitude.HasValue && (dto.Longitude < -180 || dto.Longitude > 180))
    {
        return ApiResponse<FieldDto>.Fail("Longitude must be between -180 and 180 degrees");
    }
    if ((dto.Latitude.HasValue && !dto.Longitude.HasValue) || (!dto.Latitude.HasValue && dto.Longitude.HasValue))
    {
        return ApiResponse<FieldDto>.Fail("Both Latitude and Longitude must be provided together");
    }

    // Check if field name already exists (excluding current field)
    if (!string.IsNullOrWhiteSpace(dto.FieldName) && 
        dto.FieldName != field.FieldName &&
        await _fieldRepository.FieldNameExistsAsync(dto.FieldName, farmId, id))
    {
        return ApiResponse<FieldDto>.Fail($"Field with name '{dto.FieldName}' already exists");
    }

    // Update properties
    if (!string.IsNullOrWhiteSpace(dto.FieldName)) field.FieldName = dto.FieldName;
    if (dto.Location != null) field.Location = dto.Location;
    if (dto.AreaHectares.HasValue) field.AreaHectares = dto.AreaHectares;
    if (!string.IsNullOrWhiteSpace(dto.SoilType)) 
        field.SoilType = Enum.TryParse<SoilTypeEnum>(dto.SoilType, true, out var soilType) ? soilType : field.SoilType;
    if (!string.IsNullOrWhiteSpace(dto.Status))
        field.Status = Enum.TryParse<FieldStatusEnum>(dto.Status, true, out var status) ? status : field.Status;
    if (dto.Latitude.HasValue) field.Latitude = dto.Latitude;      // ADD THIS
    if (dto.Longitude.HasValue) field.Longitude = dto.Longitude;    // ADD THIS
    
    field.UpdatedAt = DateTime.UtcNow;
    field.UpdatedBy = adminId;

    await _fieldRepository.UpdateAsync(field);
    
    var result = _mapper.Map<FieldDto>(field);
    result.ActiveCropCount = await _fieldRepository.GetActiveCropsCountAsync(field.Id);
    
    return ApiResponse<FieldDto>.Ok(result, "Field updated successfully");
}
    public async Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var field = await _fieldRepository.GetByIdAsync(id, farmId, true);
        if (field == null)
        {
            return ApiResponse<bool>.Fail($"Field with ID {id} not found");
        }

        if (field.IsDeleted)
        {
            return ApiResponse<bool>.Fail($"Field with ID {id} is already deleted");
        }

        // Check if field has active crop cycles
        var activeCrops = await _fieldRepository.GetActiveCropsCountAsync(id);
        if (activeCrops > 0)
        {
            return ApiResponse<bool>.Fail($"Cannot delete field with {activeCrops} active crop cycles. Harvest or complete them first.");
        }

        await _fieldRepository.SoftDeleteAsync(field, adminId);
        
        // Audit log
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "Field", field.Id, field, ipAddress, userAgent);
        
        return ApiResponse<bool>.Ok(true, "Field deleted successfully");
    }


    public async Task<ApiResponse<FieldDto>> GetByIdAsync(int id, int farmId)
    {
        var field = await _fieldRepository.GetByIdAsync(id, farmId);
        if (field == null)
        {
            return ApiResponse<FieldDto>.Fail($"Field with ID {id} not found");
        }

        var result = _mapper.Map<FieldDto>(field);
        result.ActiveCropCount = await _fieldRepository.GetActiveCropsCountAsync(field.Id);
        result.FarmName = field.Farm?.FarmName ?? string.Empty;
        
        return ApiResponse<FieldDto>.Ok(result);
    }

    public async Task<ApiResponse<PagedResult<FieldDto>>> GetAllAsync(FieldFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _fieldRepository.GetPagedAsync(
            farmId,
            filter.FieldName,
             filter.Location, 
            filter.SoilType,
            filter.Status,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<FieldDto>>(pagedResult.Items);
        
        // Set active crop counts
        foreach (var dto in dtos)
        {
            dto.ActiveCropCount = await _fieldRepository.GetActiveCropsCountAsync(dto.Id);
        }

        var result = new PagedResult<FieldDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<FieldDto>>.Ok(result);
    }

    public async Task<ApiResponse<FieldStatisticsDto>> GetStatisticsAsync(int farmId)
    {
        var totalArea = await _fieldRepository.GetTotalAreaAsync(farmId);
        var totalAreaWithDeleted = await _fieldRepository.GetTotalAreaAsync(farmId, true);
        var soilTypeDistribution = await _fieldRepository.GetSoilTypeDistributionAsync(farmId);
        
        var statistics = new FieldStatisticsDto
        {
            TotalFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, string.Empty),
            TotalAreaHectares = totalArea,
            ActiveFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, "ACTIVE"),
            FallowFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, "FALLOW"),
            PreparingFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, "PREPARING"),
            MaintenanceFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, "MAINTENANCE"),
            RetiredFields = await _fieldRepository.GetFieldsCountByStatusAsync(farmId, "RETIRED"),
            SoilTypeDistribution = soilTypeDistribution
        };

        return ApiResponse<FieldStatisticsDto>.Ok(statistics);
    }

// AgriculturePlatform.Application/Services/FieldService.cs

public async Task<ApiResponse<BulkImportResultDto>> BulkImportAsync(Stream fileStream, int farmId, int adminId, string ipAddress, string userAgent)
{
    var result = new BulkImportResultDto();
    var fields = await _excelService.ReadFieldsFromExcelAsync(fileStream);
    
    result.TotalRecords = fields.Count;
    var validFields = new List<Field>();
    var validator = new CreateFieldValidator();

    for (int i = 0; i < fields.Count; i++)
    {
        var fieldDto = fields[i];
        var validationResult = await validator.ValidateAsync(fieldDto);
        
        if (!validationResult.IsValid)
        {
            result.FailedCount++;
            result.Errors.Add(new BulkImportError
            {
                RowNumber = i + 2,
                FieldName = fieldDto.FieldName,
                ErrorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            });
            continue;
        }

        // Check if field name already exists
        if (await _fieldRepository.FieldNameExistsAsync(fieldDto.FieldName, farmId))
        {
            result.FailedCount++;
            result.Errors.Add(new BulkImportError
            {
                RowNumber = i + 2,
                FieldName = fieldDto.FieldName,
                ErrorMessage = "Field name already exists"
            });
            continue;
        }

        validFields.Add(new Field
        {
            FarmId = farmId,
            AdminId = adminId,     // ADD THIS - Critical for foreign key
            CreatedBy = adminId,    // Keep this for audit
            FieldName = fieldDto.FieldName,
            Location = fieldDto.Location,
            AreaHectares = fieldDto.AreaHectares,
            SoilType = Enum.TryParse<SoilTypeEnum>(fieldDto.SoilType, true, out var soilType) ? soilType : null,
            Status = Enum.TryParse<FieldStatusEnum>(fieldDto.Status, true, out var status) ? status : FieldStatusEnum.ACTIVE,
            Latitude = fieldDto.Latitude,      // ADD THIS
    Longitude = fieldDto.Longitude, 
            CreatedAt = DateTime.UtcNow
        });
    }

    if (validFields.Any())
    {
        var savedCount = await _fieldRepository.BulkCreateAsync(validFields);
        result.SuccessCount = savedCount;
        
        // Audit log for bulk import
        await _auditLogService.LogAsync(farmId, adminId, null, "BULK_IMPORT", "Field", null, null, new { Count = savedCount }, ipAddress, userAgent);
    }

    return ApiResponse<BulkImportResultDto>.Ok(result, 
        $"Imported {result.SuccessCount} of {result.TotalRecords} fields successfully");
}    public async Task<ApiResponse<byte[]>> ExportToExcelAsync(int farmId)
    {
        var fields = await _fieldRepository.GetAllAsync(farmId);
        var dtos = _mapper.Map<List<FieldDto>>(fields);
        
        foreach (var dto in dtos)
        {
            dto.ActiveCropCount = await _fieldRepository.GetActiveCropsCountAsync(dto.Id);
        }
        
        var excelBytes = await _excelService.ExportFieldsToExcelAsync(dtos);
        return ApiResponse<byte[]>.Ok(excelBytes);
    }

    public async Task<ApiResponse<BulkImportResultDto>> BulkSoftDeleteAsync(List<int> ids, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var result = new BulkImportResultDto
        {
            TotalRecords = ids.Count
        };

        var deletedCount = await _fieldRepository.BulkSoftDeleteAsync(ids, farmId, adminId);
        result.SuccessCount = deletedCount;
        result.FailedCount = ids.Count - deletedCount;

        // Audit log
        await _auditLogService.LogAsync(farmId, adminId, null, "BULK_SOFT_DELETE", "Field", null, null, new { DeletedIds = ids, Count = deletedCount }, ipAddress, userAgent);

        return ApiResponse<BulkImportResultDto>.Ok(result, $"Soft deleted {deletedCount} of {ids.Count} fields");
    }

    public async Task<bool> ValidateFieldOwnershipAsync(int fieldId, int farmId)
    {
        return await _fieldRepository.ExistsAsync(fieldId, farmId);
    }

    // Add this method to FieldService.cs

public async Task<ApiResponse<bool>> UpdateFieldLocationAsync(int id, double latitude, double longitude, int farmId, int adminId, string ipAddress, string userAgent)
{
    var field = await _fieldRepository.GetByIdAsync(id, farmId);
    if (field == null)
    {
        return ApiResponse<bool>.Fail($"Field with ID {id} not found");
    }

    // Validate coordinates
    if (latitude < -90 || latitude > 90)
        return ApiResponse<bool>.Fail("Latitude must be between -90 and 90");
    if (longitude < -180 || longitude > 180)
        return ApiResponse<bool>.Fail("Longitude must be between -180 and 180");

    var oldLatitude = field.Latitude;
    var oldLongitude = field.Longitude;

    field.Latitude = latitude;
    field.Longitude = longitude;
    field.UpdatedAt = DateTime.UtcNow;
    field.UpdatedBy = adminId;

    await _fieldRepository.UpdateAsync(field);

    // Audit log
    await _auditLogService.LogAsync(farmId, adminId, null, "UPDATE_LOCATION", "Field", field.Id, 
        new { Latitude = oldLatitude, Longitude = oldLongitude }, 
        new { Latitude = latitude, Longitude = longitude }, 
        ipAddress, userAgent);

    return ApiResponse<bool>.Ok(true, "Field location updated successfully");
}

}