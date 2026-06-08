// AgriculturePlatform.Application/Services/WorkerFieldAssignmentService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.WorkerField;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
// Remove the using for DTOs.Worker to avoid ambiguity
// using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Services;

public class WorkerFieldAssignmentService : IWorkerFieldAssignmentService
{
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public WorkerFieldAssignmentService(
        IWorkerFieldAssignmentRepository assignmentRepository,
        IWorkerRepository workerRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _workerRepository = workerRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    // =============================================
    // ADMIN OPERATIONS
    // =============================================

    public async Task<ApiResponse<WorkerFieldAssignmentDto>> AssignFieldToWorkerAsync(AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        // Validate adminId
        if (adminId <= 0)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail("Invalid admin ID. Please login again.");
        }

        // Validate worker exists
        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId, farmId);
        if (worker == null)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Worker with ID {dto.WorkerId} not found");
        }

        // Validate field exists
        var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
        if (field == null)
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field with ID {dto.FieldId} not found");
        }

        // Check if already assigned
        if (await _assignmentRepository.IsFieldAssignedToWorkerAsync(dto.FieldId, dto.WorkerId, farmId))
        {
            return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field '{field.FieldName}' is already assigned to worker '{worker.Name}'");
        }

        // Create assignment
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

        // Audit log with ipAddress and userAgent
        await _auditLogService.LogCreateAsync(farmId, adminId, "WorkerFieldAssignment", created.Id, created, ipAddress, userAgent);

        var result = _mapper.Map<WorkerFieldAssignmentDto>(created);
        return ApiResponse<WorkerFieldAssignmentDto>.Ok(result, "Field assigned to worker successfully");
    }

    public async Task<ApiResponse<WorkerFieldAssignmentDto>> UpdateAssignmentAsync(int id, AssignFieldToWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent)
    {
        // Validate adminId
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

        // Update fields
        if (dto.WorkerId > 0 && dto.WorkerId != assignment.WorkerId)
        {
            var worker = await _workerRepository.GetByIdAsync(dto.WorkerId, farmId);
            if (worker == null)
            {
                return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Worker with ID {dto.WorkerId} not found");
            }
            assignment.WorkerId = dto.WorkerId;
        }

        if (dto.FieldId > 0 && dto.FieldId != assignment.FieldId)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
            if (field == null)
            {
                return ApiResponse<WorkerFieldAssignmentDto>.Fail($"Field with ID {dto.FieldId} not found");
            }
            assignment.FieldId = dto.FieldId;
        }

        if (dto.EndDate.HasValue)
            assignment.EndDate = dto.EndDate;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            assignment.Notes = dto.Notes;

        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = adminId;

        await _assignmentRepository.UpdateAsync(assignment);

        // Audit log
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

    // =============================================
    // WORKER OPERATIONS
    // =============================================

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