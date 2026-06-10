// Application/Interfaces/IHarvestService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;

namespace AgriculturePlatform.Application.Interfaces;

public interface IHarvestService
{
    // Worker Operations
    Task<ApiResponse<HarvestDto>> CreateHarvestAsync(CreateHarvestDto dto, int farmId, int workerId, int adminId);
    Task<ApiResponse<HarvestDto>> UpdateOwnHarvestAsync(int id, UpdateHarvestDto dto, int workerId, int farmId);
    Task<ApiResponse<bool>> DeleteOwnHarvestAsync(int id, int workerId, int farmId);
    Task<ApiResponse<HarvestDto>> RespondToAdminAsync(int id, HarvestWorkerResponseDto response, int farmId, int workerId);
    
    // Admin Operations
    Task<ApiResponse<HarvestDto>> UpdateHarvestAsync(int id, UpdateHarvestDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteHarvestAsync(int id, int farmId, int adminId);
    Task<ApiResponse<HarvestDto>> ApproveHarvestAsync(int id, HarvestApprovalDto approval, int farmId, int adminId);
    Task<ApiResponse<PagedResult<HarvestDto>>> GetAllHarvestsAsync(HarvestFilterDto filter, int farmId);
    Task<ApiResponse<HarvestDto>> GetHarvestByIdAsync(int id, int farmId);
    Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByFieldAsync(int fieldId, int farmId);
    Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByCropCycleAsync(int cropCycleId, int farmId);
    Task<ApiResponse<IEnumerable<HarvestDto>>> GetHarvestsByWorkerAsync(int workerId, int farmId);
    Task<ApiResponse<PagedResult<HarvestDto>>> GetPendingApprovalsAsync(int farmId, PaginationParams pagination);
    
    // Statistics
    Task<ApiResponse<YieldStatisticsDto>> GetYieldStatisticsAsync(int farmId, int? cropCycleId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<YieldStatisticsDto>> GetYearOverYearComparisonAsync(int farmId, int currentYear, int? previousYear);
    
    // Validation
    Task<bool> ValidateHarvestOwnershipAsync(int harvestId, int workerId, int farmId);
    Task<bool> HasPendingApprovalsAsync(int workerId, int farmId);
}