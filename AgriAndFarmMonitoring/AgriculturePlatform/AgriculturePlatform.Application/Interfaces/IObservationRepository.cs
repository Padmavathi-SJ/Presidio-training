using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

// Application/Interfaces/IObservationRepository.cs
namespace AgriculturePlatform.Application.Interfaces;

public interface IObservationRepository
{
    // Basic CRUD
    Task<Observation?> GetByIdAsync(int id, int farmId);
    Task<Observation> CreateAsync(Observation observation);
    Task UpdateAsync(Observation observation);
    Task SoftDeleteAsync(Observation observation, int deletedBy);
    Task<bool> ExistsAsync(int id, int farmId);
    
    // Query methods
    Task<PagedResult<Observation>> GetPagedAsync(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        int? workerId,
        string? cropHealth,
        DateTime? fromDate,
        DateTime? toDate,
        string? validationStatus,  // NEW
        bool includeDeleted,
        PaginationParams paginationParams);
    
    Task<IEnumerable<Observation>> GetByFieldAsync(int fieldId, int farmId);
    Task<IEnumerable<Observation>> GetByCropCycleAsync(int cropCycleId, int farmId);
    Task<IEnumerable<Observation>> GetByWorkerAsync(int workerId, int farmId);
    Task<IEnumerable<Observation>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate);
    
    // NEW: Validation methods
    Task<IEnumerable<Observation>> GetPendingValidationsAsync(int farmId);
    Task<IEnumerable<Observation>> GetQuestionedObservationsAsync(int farmId);
    
    // Statistics
    Task<ObservationStatisticsDto> GetPestDetectionStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    Task<Dictionary<string, int>> GetPestTypeDistributionAsync(int farmId);
    
    // Ownership validation
    Task<bool> IsOwnerAsync(int observationId, int workerId, int farmId);

    // NEW: Count by validation status
    Task<int> CountByValidationStatusAsync(int farmId, string validationStatus);
}