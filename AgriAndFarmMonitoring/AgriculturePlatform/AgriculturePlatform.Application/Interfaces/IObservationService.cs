// Application/Interfaces/IObservationService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;

namespace AgriculturePlatform.Application.Interfaces;

public interface IObservationService
{
    // Worker operations
    Task<ApiResponse<ObservationDto>> CreateObservationAsync(CreateObservationDto dto, int farmId, int workerId, int adminId);
    Task<ApiResponse<ObservationDto>> UpdateOwnObservationAsync(int id, UpdateObservationDto dto, int workerId, int farmId);
    Task<ApiResponse<bool>> DeleteOwnObservationAsync(int id, int workerId, int farmId);
    
    // NEW: Worker responds to admin questions
    Task<ApiResponse<ObservationDto>> RespondToAdminAsync(int id, ObservationWorkerResponseDto response, int farmId, int workerId);
     Task<ApiResponse<ObservationDto>> PatchObservationAsync(int id, UpdateObservationDto dto, int workerId, int farmId);
    
    // Admin operations
    Task<ApiResponse<ObservationDto>> UpdateObservationAsync(int id, UpdateObservationDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteObservationAsync(int id, int farmId, int adminId);
    Task<ApiResponse<PagedResult<ObservationDto>>> GetAllObservationsAsync(ObservationFilterDto filter, int farmId);
    Task<ApiResponse<ObservationDto>> GetObservationByIdAsync(int id, int farmId);
    Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByFieldAsync(int fieldId, int farmId);
    Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByCropCycleAsync(int cropCycleId, int farmId);
    Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByWorkerAsync(int workerId, int farmId);
    Task<ApiResponse<IEnumerable<ObservationDto>>> GetObservationsByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate);
    
    // NEW: Admin validation operations
    Task<ApiResponse<ObservationDto>> ValidateObservationAsync(int id, ObservationValidationDto validation, int farmId, int adminId);
    Task<ApiResponse<PagedResult<ObservationDto>>> GetPendingValidationsAsync(int farmId, PaginationParams pagination);
    Task<ApiResponse<PagedResult<ObservationDto>>> GetQuestionedObservationsAsync(int farmId, PaginationParams pagination);
    
    // Statistics
    Task<ApiResponse<ObservationStatisticsDto>> GetPestStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate, int? workerId = null);
    
    // Validation
    Task<bool> ValidateObservationOwnershipAsync(int observationId, int workerId, int farmId);
}