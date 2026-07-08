// Application/Interfaces/IQualityCheckService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;

namespace AgriculturePlatform.Application.Interfaces;

public interface IQualityCheckService
{
    // Worker Operations
    Task<ApiResponse<QualityCheckDto>> CreateQualityCheckAsync(CreateQualityCheckDto dto, int farmId, int workerId, int adminId);
    Task<ApiResponse<QualityCheckDto>> UpdateOwnQualityCheckAsync(int id, UpdateQualityCheckDto dto, int workerId, int farmId);
    Task<ApiResponse<bool>> DeleteOwnQualityCheckAsync(int id, int workerId, int farmId);
    Task<ApiResponse<QualityCheckDto>> RespondToAdminAsync(int id, QualityCheckWorkerResponseDto response, int farmId, int workerId);
    
    // Admin Operations
    Task<ApiResponse<QualityCheckDto>> UpdateQualityCheckAsync(int id, UpdateQualityCheckDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteQualityCheckAsync(int id, int farmId, int adminId);
    Task<ApiResponse<QualityCheckDto>> ApproveQualityCheckAsync(int id, QualityCheckApprovalDto approval, int farmId, int adminId);
    Task<ApiResponse<PagedResult<QualityCheckDto>>> GetAllQualityChecksAsync(QualityCheckFilterDto filter, int farmId);
    Task<ApiResponse<QualityCheckDto>> GetQualityCheckByIdAsync(int id, int farmId);
    Task<ApiResponse<IEnumerable<QualityCheckDto>>> GetQualityChecksByHarvestAsync(int harvestId, int farmId);
    Task<ApiResponse<IEnumerable<QualityCheckDto>>> GetQualityChecksByWorkerAsync(int workerId, int farmId);
    Task<ApiResponse<PagedResult<QualityCheckDto>>> GetPendingApprovalsAsync(int farmId, PaginationParams pagination);
    
    // Statistics
    Task<ApiResponse<QualityStatisticsDto>> GetQualityStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate, int? workerId = null);
    
    // Validation
    Task<bool> ValidateQualityCheckOwnershipAsync(int qualityCheckId, int workerId, int farmId);
    Task<bool> HasPendingApprovalsAsync(int workerId, int farmId);
}