// Application/Services/QualityCheckService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class QualityCheckService : IQualityCheckService
{
    private readonly IQualityCheckRepository _qualityCheckRepository;
    private readonly IHarvestRepository _harvestRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public QualityCheckService(
        IQualityCheckRepository qualityCheckRepository,
        IHarvestRepository harvestRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _qualityCheckRepository = qualityCheckRepository;
        _harvestRepository = harvestRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    // =============================================
    // WORKER OPERATIONS
    // =============================================

    public async Task<ApiResponse<QualityCheckDto>> CreateQualityCheckAsync(CreateQualityCheckDto dto, int farmId, int workerId, int adminId)
    {
        // Validate harvest exists
        var harvest = await _harvestRepository.GetByIdAsync(dto.HarvestId, farmId);
        if (harvest == null)
            return ApiResponse<QualityCheckDto>.Fail($"Harvest with ID {dto.HarvestId} not found");

        var qualityCheck = new QualityCheck
        {
            FarmId = farmId,
            AdminId = adminId,
            HarvestId = dto.HarvestId,
            CheckedBy = workerId,
            CheckDate = dto.CheckDate.ToUniversalTime(),
            MoisturePct = dto.MoisturePct,
            DefectPct = dto.DefectPct,
             Notes = dto.Notes,
            ApprovalStatus = "PENDING",
            CreatedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.FinalGrade))
            qualityCheck.FinalGrade = Enum.Parse<QualityGradeEnum>(dto.FinalGrade, true);

        var created = await _qualityCheckRepository.CreateAsync(qualityCheck);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "QualityCheck", created.Id, created, null, null);

        var result = _mapper.Map<QualityCheckDto>(created);
        return ApiResponse<QualityCheckDto>.Ok(result, "Quality check submitted for approval");
    }

    public async Task<ApiResponse<QualityCheckDto>> UpdateOwnQualityCheckAsync(int id, UpdateQualityCheckDto dto, int workerId, int farmId)
    {
        if (!await _qualityCheckRepository.CanWorkerEditAsync(id, workerId, farmId))
        {
            return ApiResponse<QualityCheckDto>.Fail("You don't have permission to update this quality check. Only pending or requested changes checks can be edited.");
        }

        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<QualityCheckDto>.Fail($"Quality check with ID {id} not found");

        var oldCheck = _mapper.Map<QualityCheck>(qualityCheck);

        if (dto.CheckDate.HasValue)
            qualityCheck.CheckDate = dto.CheckDate.Value.ToUniversalTime();
        if (dto.MoisturePct.HasValue)
            qualityCheck.MoisturePct = dto.MoisturePct.Value;
        if (dto.DefectPct.HasValue)
            qualityCheck.DefectPct = dto.DefectPct.Value;
        if (!string.IsNullOrWhiteSpace(dto.FinalGrade))
            qualityCheck.FinalGrade = Enum.Parse<QualityGradeEnum>(dto.FinalGrade, true);
        if (!string.IsNullOrWhiteSpace(dto.Notes))
    qualityCheck.Notes = dto.Notes; 

        qualityCheck.UpdatedAt = DateTime.UtcNow;
        qualityCheck.UpdatedBy = workerId;
        
        if (qualityCheck.ApprovalStatus == "REQUEST_CHANGES")
        {
            qualityCheck.ApprovalStatus = "PENDING";
            qualityCheck.WorkerResponse = null;
        }

        await _qualityCheckRepository.UpdateAsync(qualityCheck);
        
        await _auditLogService.LogUpdateAsync(farmId, null, "QualityCheck", qualityCheck.Id, oldCheck, qualityCheck, null, null);

        var result = _mapper.Map<QualityCheckDto>(qualityCheck);
        return ApiResponse<QualityCheckDto>.Ok(result, "Quality check updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteOwnQualityCheckAsync(int id, int workerId, int farmId)
    {
        if (!await _qualityCheckRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<bool>.Fail("You don't have permission to delete this quality check");
        }

        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<bool>.Fail($"Quality check with ID {id} not found");

        if (qualityCheck.ApprovalStatus == "APPROVED")
        {
            return ApiResponse<bool>.Fail("Cannot delete an approved quality check. Please contact an admin.");
        }

        await _qualityCheckRepository.SoftDeleteAsync(qualityCheck, workerId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, null, "QualityCheck", qualityCheck.Id, qualityCheck, null, null);

        return ApiResponse<bool>.Ok(true, "Quality check deleted successfully");
    }

    public async Task<ApiResponse<QualityCheckDto>> RespondToAdminAsync(int id, QualityCheckWorkerResponseDto response, int farmId, int workerId)
    {
        if (!await _qualityCheckRepository.IsOwnerAsync(id, workerId, farmId))
        {
            return ApiResponse<QualityCheckDto>.Fail("You don't have permission to respond");
        }

        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<QualityCheckDto>.Fail($"Quality check with ID {id} not found");
        
        if (qualityCheck.ApprovalStatus != "REQUEST_CHANGES")
        {
            return ApiResponse<QualityCheckDto>.Fail("Can only respond to quality checks that need changes");
        }

        qualityCheck.WorkerResponse = response.WorkerResponse;
        qualityCheck.ApprovalStatus = "PENDING";
        qualityCheck.UpdatedAt = DateTime.UtcNow;
        qualityCheck.UpdatedBy = workerId;
        
        await _qualityCheckRepository.UpdateAsync(qualityCheck);

        var result = _mapper.Map<QualityCheckDto>(qualityCheck);
        return ApiResponse<QualityCheckDto>.Ok(result, "Response submitted");
    }

    // =============================================
    // ADMIN OPERATIONS
    // =============================================

    public async Task<ApiResponse<QualityCheckDto>> UpdateQualityCheckAsync(int id, UpdateQualityCheckDto dto, int farmId, int adminId)
    {
        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<QualityCheckDto>.Fail($"Quality check with ID {id} not found");

        var oldCheck = _mapper.Map<QualityCheck>(qualityCheck);

        if (dto.CheckDate.HasValue)
            qualityCheck.CheckDate = dto.CheckDate.Value.ToUniversalTime();
        if (dto.MoisturePct.HasValue)
            qualityCheck.MoisturePct = dto.MoisturePct.Value;
        if (dto.DefectPct.HasValue)
            qualityCheck.DefectPct = dto.DefectPct.Value;
        if (!string.IsNullOrWhiteSpace(dto.FinalGrade))
            qualityCheck.FinalGrade = Enum.Parse<QualityGradeEnum>(dto.FinalGrade, true);

        qualityCheck.UpdatedAt = DateTime.UtcNow;
        qualityCheck.UpdatedBy = adminId;

        await _qualityCheckRepository.UpdateAsync(qualityCheck);
        
        await _auditLogService.LogUpdateAsync(farmId, adminId, "QualityCheck", qualityCheck.Id, oldCheck, qualityCheck, null, null);

        var result = _mapper.Map<QualityCheckDto>(qualityCheck);
        return ApiResponse<QualityCheckDto>.Ok(result, "Quality check updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteQualityCheckAsync(int id, int farmId, int adminId)
    {
        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<bool>.Fail($"Quality check with ID {id} not found");

        await _qualityCheckRepository.SoftDeleteAsync(qualityCheck, adminId);
        
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "QualityCheck", qualityCheck.Id, qualityCheck, null, null);

        return ApiResponse<bool>.Ok(true, "Quality check deleted successfully");
    }

// In QualityCheckService.cs - ApproveQualityCheckAsync
public async Task<ApiResponse<QualityCheckDto>> ApproveQualityCheckAsync(int id, QualityCheckApprovalDto approval, int farmId, int adminId)
{
    var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
    if (qualityCheck == null)
        return ApiResponse<QualityCheckDto>.Fail($"Quality check with ID {id} not found");

    var oldStatus = qualityCheck.ApprovalStatus;
    
    qualityCheck.ApprovalStatus = approval.ApprovalStatus;
    qualityCheck.ApprovedBy = adminId;
    qualityCheck.ApprovedAt = DateTime.UtcNow;
    qualityCheck.AdminNotes = approval.AdminNotes;
    
    if (approval.ApprovalStatus == "REJECTED")
    {
        qualityCheck.RejectionReason = approval.RejectionReason;
    }
    
    qualityCheck.UpdatedAt = DateTime.UtcNow;
    qualityCheck.UpdatedBy = adminId;
    
    await _qualityCheckRepository.UpdateAsync(qualityCheck);
    
    // ✅ Fetch the updated entity with navigation properties
    var updatedCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
    
    await _auditLogService.LogUpdateAsync(farmId, adminId, "QualityCheck", qualityCheck.Id, 
        new { ApprovalStatus = oldStatus }, 
        new { ApprovalStatus = approval.ApprovalStatus }, null, null);

    var result = _mapper.Map<QualityCheckDto>(updatedCheck ?? qualityCheck);
    return ApiResponse<QualityCheckDto>.Ok(result, $"Quality check {approval.ApprovalStatus.ToLower()}");
}
    public async Task<ApiResponse<PagedResult<QualityCheckDto>>> GetAllQualityChecksAsync(QualityCheckFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _qualityCheckRepository.GetPagedAsync(
            farmId,
            filter.HarvestId,
            filter.WorkerId,
            filter.ApprovalStatus,
            filter.FinalGrade,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted ?? false,
            paginationParams);

        var dtos = _mapper.Map<List<QualityCheckDto>>(pagedResult.Items);
        
        var result = new PagedResult<QualityCheckDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<QualityCheckDto>>.Ok(result);
    }

    public async Task<ApiResponse<QualityCheckDto>> GetQualityCheckByIdAsync(int id, int farmId)
    {
        var qualityCheck = await _qualityCheckRepository.GetByIdAsync(id, farmId);
        if (qualityCheck == null)
            return ApiResponse<QualityCheckDto>.Fail($"Quality check with ID {id} not found");

        var result = _mapper.Map<QualityCheckDto>(qualityCheck);
        return ApiResponse<QualityCheckDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<QualityCheckDto>>> GetQualityChecksByHarvestAsync(int harvestId, int farmId)
    {
        var checks = await _qualityCheckRepository.GetByHarvestAsync(harvestId, farmId);
        var dtos = _mapper.Map<IEnumerable<QualityCheckDto>>(checks);
        return ApiResponse<IEnumerable<QualityCheckDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<QualityCheckDto>>> GetQualityChecksByWorkerAsync(int workerId, int farmId)
    {
        var checks = await _qualityCheckRepository.GetByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<QualityCheckDto>>(checks);
        return ApiResponse<IEnumerable<QualityCheckDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PagedResult<QualityCheckDto>>> GetPendingApprovalsAsync(int farmId, PaginationParams pagination)
    {
        var checks = await _qualityCheckRepository.GetPendingApprovalsAsync(farmId);
        
        var paged = checks
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();
        
        var dtos = _mapper.Map<List<QualityCheckDto>>(paged);
        
        var result = new PagedResult<QualityCheckDto>
        {
            Items = dtos,
            TotalCount = checks.Count(),
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        
        return ApiResponse<PagedResult<QualityCheckDto>>.Ok(result);
    }

    // =============================================
    // STATISTICS
    // =============================================

    public async Task<ApiResponse<QualityStatisticsDto>> GetQualityStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate, int? workerId = null)
    {
        var stats = await _qualityCheckRepository.GetQualityStatisticsAsync(farmId, fromDate, toDate, workerId);
        return ApiResponse<QualityStatisticsDto>.Ok(stats);
    }

    // =============================================
    // VALIDATION
    // =============================================

    public async Task<bool> ValidateQualityCheckOwnershipAsync(int qualityCheckId, int workerId, int farmId)
    {
        return await _qualityCheckRepository.IsOwnerAsync(qualityCheckId, workerId, farmId);
    }

    public async Task<bool> HasPendingApprovalsAsync(int workerId, int farmId)
    {
        return await _qualityCheckRepository.HasPendingApprovalAsync(workerId, farmId);
    }
}