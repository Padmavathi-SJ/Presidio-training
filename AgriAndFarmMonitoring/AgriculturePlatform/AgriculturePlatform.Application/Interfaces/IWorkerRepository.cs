// AgriculturePlatform.Application/Interfaces/IWorkerRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerRepository
{
    // Basic CRUD
    Task<Worker?> GetByIdAsync(int id, int farmId, bool includeDeleted = false);
    Task<IEnumerable<Worker>> GetAllAsync(int farmId, bool includeDeleted = false);
    Task<Worker> CreateAsync(Worker worker);
    Task UpdateAsync(Worker worker);
    Task SoftDeleteAsync(Worker worker, int deletedBy);
    Task<bool> ExistsAsync(int id, int farmId);
    Task<Worker?> GetByEmailAsync(string email);
    
    // Query methods
    Task<bool> EmailExistsAsync(string email, int farmId, int? excludeId = null);
    
    // Filtering & Pagination
    Task<PagedResult<Worker>> GetPagedAsync(
        int farmId,
        string? name,
        string? email,
        string? role,
        bool? isActive,
        DateTime? hireDateFrom,
        DateTime? hireDateTo,
        bool includeDeleted,
        PaginationParams paginationParams);
    
    // Statistics
    Task<int> GetActiveWorkersCountAsync(int farmId);
    Task<Dictionary<string, int>> GetWorkersByRoleDistributionAsync(int farmId);
    Task<DateTime?> GetLastLoginAsync(int workerId);
    
    // Password management
    Task UpdatePasswordAsync(int workerId, string passwordHash);
    
    // Login tracking
    Task RecordLoginAsync(int workerId, string ipAddress);

    Task<Worker?> GetWorkerWithFarmAsync(int workerId, int farmId);
    Task<bool> UpdateWorkerProfileAsync(Worker worker);
    Task<bool> UpdateWorkerPasswordAsync(int workerId, string newPasswordHash);
    Task<Worker?> GetByNameAsync(string name, int farmId);
}