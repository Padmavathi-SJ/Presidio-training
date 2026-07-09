// AgriculturePlatform.Application/Services/WorkerFieldAssignmentService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.WorkerField;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Services;

public class WorkerFieldAssignmentService : IWorkerFieldAssignmentService
{
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IWorkerRepository _workerRepository;  // ✅ ADD THIS - Was missing
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public WorkerFieldAssignmentService(
        IWorkerFieldAssignmentRepository assignmentRepository,
        IWorkerRepository workerRepository,  // ✅ Fix parameter name
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _workerRepository = workerRepository;  // ✅ Assign to the field
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<WorkerFieldAssignmentDto>> AssignFieldToWorkerAsync(AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        if (adminId <= 0)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail("Invalid admin ID. Please login again.");
        }

        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId, farmId);
        if (worker == null)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Worker with ID {dto.WorkerId} not found");
        }

        var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
        if (field == null)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field with ID {dto.FieldId} not found");
        }

        if (await _assignmentRepository.IsFieldAssignedToWorkerAsync(dto.FieldId, dto.WorkerId, farmId))
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field '{field.FieldName}' is already assigned to worker '{worker.Name}'");
        }

        var assignment = new WorkerFieldAssignment
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = dto.WorkerId,
            FieldId = dto.FieldId,
            IsActive = true,
            AssignedDate = dto.AssignedDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate,
            Notes = dto.Notes,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _assignmentRepository.CreateAsync(assignment);

        await _auditLogService.LogCreateAsync(farmId, adminId, "WorkerFieldAssignment", created.Id, created, ipAddress, userAgent);

        var dateRangeStr = created.EndDate.HasValue 
            ? $"from {created.AssignedDate:MMM dd, yyyy} to {created.EndDate.Value:MMM dd, yyyy}" 
            : $"starting {created.AssignedDate:MMM dd, yyyy}";
            
        await _notificationService.CreateNotificationAsync(
            farmId, 
            null, 
            dto.WorkerId, 
            "New Field Assigned", 
            $"You have been assigned to field '{field.FieldName}' {dateRangeStr}.", 
            "FieldAssignment",
            "/worker/fields"
        );

        var result = _mapper.Map<WorkerFieldAssignmentDto>(created);
        return ApiResponse<WorkerFieldAssignmentDto>.Ok(result, "Field assigned to worker successfully");
    }

    public async Task<ApiResponse<WorkerFieldAssignmentDto>> UpdateAssignmentAsync(int id, AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        if (adminId <= 0)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail("Invalid admin ID. Please login again.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(id, farmId);
        if (assignment == null)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Assignment with ID {id} not found");
        }

        var oldAssignment = _mapper.Map<WorkerFieldAssignment>(assignment);

        // Update Worker
        if (dto.WorkerId > 0 && dto.WorkerId != assignment.WorkerId)
        {
            var worker = await _workerRepository.GetByIdAsync(dto.WorkerId, farmId);
            if (worker == null)
            {
                return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Worker with ID {dto.WorkerId} not found");
            }
            assignment.WorkerId = dto.WorkerId;
        }

        // Update Field
        if (dto.FieldId > 0 && dto.FieldId != assignment.FieldId)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
            if (field == null)
            {
                return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field with ID {dto.FieldId} not found");
            }
            assignment.FieldId = dto.FieldId;
        }

        // Update Assigned Date
        if (dto.AssignedDate.HasValue)
        {
            assignment.AssignedDate = dto.AssignedDate.Value.ToUniversalTime();
        }

        // Update End Date
        if (dto.EndDate.HasValue)
        {
            assignment.EndDate = dto.EndDate.Value.ToUniversalTime();
        }
        else if (dto.EndDate == null)
        {
            assignment.EndDate = null;
        }

        // Update Notes
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            assignment.Notes = dto.Notes;
        }
        else if (dto.Notes == null)
        {
            assignment.Notes = null;
        }

        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = adminId;

        await _assignmentRepository.UpdateAsync(assignment);

        await _auditLogService.LogUpdateAsync(farmId, adminId, "WorkerFieldAssignment", assignment.Id, oldAssignment, assignment, ipAddress, userAgent);

        var result = _mapper.Map<WorkerFieldAssignmentDto>(assignment);
        return ApiResponse<WorkerFieldAssignmentDto>.Ok(result, "Assignment updated successfully");
    }

    public async Task<ApiResponse<bool>> RemoveAssignmentAsync(int id, int farmId, int adminId, string ipAddress, string userAgent)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, farmId);
        if (assignment == null)
        {
            return ApiResponse<bool>.Fail($"Assignment with ID {id} not found");
        }

        await _assignmentRepository.SoftDeleteAsync(assignment, adminId);

        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "WorkerFieldAssignment", assignment.Id, assignment, ipAddress, userAgent);

        return ApiResponse<bool>.Ok(true, "Assignment removed successfully");
    }

    public async Task<ApiResponse<PagedResult<WorkerFieldAssignmentDto>>> GetAllAssignmentsAsync(WorkerFieldFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _assignmentRepository.GetPagedAssignmentsAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.IsActive,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.EndDateFrom,
            filter.EndDateTo,
            paginationParams);

        var dtos = _mapper.Map<List<WorkerFieldAssignmentDto>>(pagedResult.Items);

        var result = new PagedResult<WorkerFieldAssignmentDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<WorkerFieldAssignmentDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<AgriculturePlatform.Application.DTOs.Worker.WorkerFieldDetailDto>>> GetMyAssignedFieldsAsync(int workerId, int farmId)
    {
        var assignments = await _assignmentRepository.GetWorkerAssignedFieldsAsync(workerId, farmId);
        
        var result = new List<AgriculturePlatform.Application.DTOs.Worker.WorkerFieldDetailDto>();
        
        foreach (var assignment in assignments)
        {
            // Get crop cycles for this field
            var cropCycles = await _cropCycleRepository.GetAllAsync(farmId, false);
            var fieldCropCycles = cropCycles
                .Where(c => c.FieldId == assignment.FieldId)
                .Select(c => new AgriculturePlatform.Application.DTOs.Worker.WorkerCropCycleDto
                {
                    Id = c.Id,
                    CropType = c.CropType?.ToString(),
                    PlantingDate = c.PlantingDate,
                    ExpectedHarvestDate = c.ExpectedHarvestDate,
                    GrowthStage = c.GrowthStage?.ToString(),
                    Status = c.Status?.ToString()
                })
                .ToList();

            result.Add(new AgriculturePlatform.Application.DTOs.Worker.WorkerFieldDetailDto
            {
                AssignmentId = assignment.Id,
                FieldId = assignment.FieldId,
                FieldName = assignment.Field?.FieldName ?? string.Empty,
                Location = assignment.Field?.Location,
                AreaHectares = assignment.Field?.AreaHectares,
                SoilType = assignment.Field?.SoilType?.ToString(),
                Status = assignment.Field?.Status?.ToString(),
                AssignedDate = assignment.AssignedDate,
                Notes = assignment.Notes,
                CreatedAt = assignment.Field?.CreatedAt ?? DateTime.UtcNow,
                CropCycles = fieldCropCycles,
                Latitude = assignment.Field?.Latitude,
                Longitude = assignment.Field?.Longitude
            });
        }

        return ApiResponse<List<AgriculturePlatform.Application.DTOs.Worker.WorkerFieldDetailDto>>.Ok(result);
    }
}