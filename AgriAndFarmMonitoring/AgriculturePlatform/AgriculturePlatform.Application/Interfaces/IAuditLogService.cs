// AgriculturePlatform.Application/Interfaces/IAuditLogService.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        int? farmId,
        int? adminId,
        int? workerId,
        string action,
        string entityType,
        int? entityId,
        object? oldValue,
        object? newValue,
        string? ipAddress = null,
        string? userAgent = null);
    
    Task LogCreateAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null);
    Task LogUpdateAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T oldEntity, T newEntity, string? ipAddress = null, string? userAgent = null);
    Task LogDeleteAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null);
    Task LogSoftDeleteAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null);

}