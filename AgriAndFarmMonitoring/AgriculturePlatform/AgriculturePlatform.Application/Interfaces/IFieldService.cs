// AgriculturePlatform.Application/Interfaces/IFieldService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Field;

namespace AgriculturePlatform.Application.Interfaces;

public interface IFieldService
{
    // Basic CRUD
    Task<ApiResponse<FieldDto>> CreateAsync(CreateFieldDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<FieldDto>> UpdateAsync(int id, UpdateFieldDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    
    // Location management - NEW
    Task<ApiResponse<bool>> UpdateFieldLocationAsync(int id, double latitude, double longitude, int farmId, int adminId, string ipAddress, string userAgent);
    
    // Query methods
    Task<ApiResponse<FieldDto>> GetByIdAsync(int id, int farmId);
    Task<ApiResponse<PagedResult<FieldDto>>> GetAllAsync(FieldFilterDto filter, int farmId);
    
    // Statistics
    Task<ApiResponse<FieldStatisticsDto>> GetStatisticsAsync(int farmId);
    
    // Bulk operations
    Task<ApiResponse<BulkImportResultDto>> BulkImportAsync(Stream fileStream, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<byte[]>> ExportToExcelAsync(int farmId);
    Task<ApiResponse<BulkImportResultDto>> BulkSoftDeleteAsync(List<int> ids, int farmId, int adminId, string ipAddress, string userAgent);
    
    // Validation
    Task<bool> ValidateFieldOwnershipAsync(int fieldId, int farmId);
}