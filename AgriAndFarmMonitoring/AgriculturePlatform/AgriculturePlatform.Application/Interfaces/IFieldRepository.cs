// AgriculturePlatform.Application/Interfaces/IFieldRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IFieldRepository
{
    // Basic CRUD
    Task<Field?> GetByIdAsync(int id, int farmId, bool includeDeleted = false);
    Task<IEnumerable<Field>> GetAllAsync(int farmId, bool includeDeleted = false);
    Task<Field> CreateAsync(Field field);
    Task UpdateAsync(Field field);
    Task SoftDeleteAsync(Field field, int deletedBy);
    
    // Query methods
    Task<bool> ExistsAsync(int id, int farmId);
    Task<bool> FieldNameExistsAsync(string fieldName, int farmId, int? excludeId = null);
    
    // Filtering & Pagination (with soft delete support)
    Task<PagedResult<Field>> GetPagedAsync(
        int farmId, 
        string? searchTerm, 
        string? location,  
        string? soilType, 
        string? status,
        bool includeDeleted,
        PaginationParams paginationParams);
    
    // Statistics
    Task<int> GetActiveCropsCountAsync(int fieldId);
    Task<decimal> GetTotalAreaAsync(int farmId, bool includeDeleted = false);
    Task<Dictionary<string, int>> GetSoilTypeDistributionAsync(int farmId, bool includeDeleted = false);
    Task<int> GetFieldsCountByStatusAsync(int farmId, string status, bool includeDeleted = false);
    
    // Bulk operations
    Task<IEnumerable<Field>> GetFieldsByIdsAsync(IEnumerable<int> ids, int farmId);
    Task<int> BulkCreateAsync(IEnumerable<Field> fields);
    Task<int> BulkSoftDeleteAsync(IEnumerable<int> ids, int farmId, int deletedBy);
    
    Task<List<Field>> GetByFarmIdAsync(int farmId);
    Task<Field?> GetByNameAsync(string fieldName, int farmId);
}