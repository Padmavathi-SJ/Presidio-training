// AgriculturePlatform.Application/Interfaces/IWorkerFieldAssignmentRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerFieldAssignmentRepository
{
    // Admin operations
    Task<WorkerFieldAssignment?> GetByIdAsync(int id, int farmId);
    Task<WorkerFieldAssignment> CreateAsync(WorkerFieldAssignment assignment);
    Task UpdateAsync(WorkerFieldAssignment assignment);
    Task SoftDeleteAsync(WorkerFieldAssignment assignment, int deletedBy);
    Task<PagedResult<WorkerFieldAssignment>> GetPagedAssignmentsAsync(
        int farmId, int? workerId, int? fieldId, bool? isActive,
        DateTime? assignedDateFrom, DateTime? assignedDateTo,
        PaginationParams paginationParams);
    
    // Worker operations
    Task<List<WorkerFieldAssignment>> GetWorkerAssignedFieldsAsync(int workerId, int farmId);
    Task<bool> IsFieldAssignedToWorkerAsync(int fieldId, int workerId, int farmId);
    Task<bool> HasWorkerAccessToFieldAsync(int workerId, int fieldId, int farmId);
    
    // Utility
    Task<bool> ExistsAsync(int id, int farmId);

    /// <summary>
/// Get all active field assignments for a worker
/// </summary>
Task<List<WorkerFieldAssignment>> GetWorkerActiveAssignmentsAsync(int workerId, int farmId);


}