// AgriculturePlatform.Application/Services/AuditLogService.cs
using System.Text.Json;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(
        int? farmId,
        int? adminId,
        int? workerId,
        string action,
        string entityType,
        int? entityId,
        object? oldValue,
        object? newValue,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = workerId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = SerializeSafe(oldValue),
            NewValue = SerializeSafe(newValue),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
        await _auditLogRepository.SaveChangesAsync();
    }

    private JsonDocument? SerializeSafe(object? obj)
    {
        if (obj == null) return null;
        
        try
        {
            // Create a DTO with only primitive properties to avoid circular references
            var safeObject = CreateSafeObject(obj);
            var jsonString = JsonSerializer.Serialize(safeObject, new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
            return JsonDocument.Parse(jsonString);
        }
        catch (Exception ex)
        {
            // If serialization fails, log as string
            Console.WriteLine($"Serialization error: {ex.Message}");
            var fallbackJson = JsonSerializer.Serialize(new { Error = "Serialization failed", Type = obj.GetType().Name });
            return JsonDocument.Parse(fallbackJson);
        }
    }

    private object? CreateSafeObject(object? obj)
    {
        if (obj == null) return null;

        // For Field entity, create a simplified DTO
        var field = obj as Domain.Entities.CropMonitoring.Field;
        if (field != null)
        {
            return new
            {
                field.Id,
                field.FieldName,
                field.Location,
                field.AreaHectares,
                field.SoilType,
                field.Status,
                field.FarmId,
                field.AdminId,
                field.CreatedAt,
                field.UpdatedAt,
                field.IsDeleted,
                field.DeletedAt
            };
        }

        // For other entities, try to extract only primitive properties
        var properties = obj.GetType().GetProperties()
            .Where(p => p.CanRead && IsPrimitiveType(p.PropertyType))
            .ToDictionary(p => p.Name, p => p.GetValue(obj));
        
        return properties;
    }

    private bool IsPrimitiveType(Type type)
    {
        return type.IsPrimitive || 
               type == typeof(string) || 
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTime?) ||
               type == typeof(int?) ||
               type == typeof(decimal?) ||
               type == typeof(bool) ||
               type == typeof(bool?) ||
               type.IsEnum;
    }

    public async Task LogCreateAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null)
    {
        await LogAsync(farmId, adminId, null, "CREATE", entityType, entityId, null, entity, ipAddress, userAgent);
    }

    public async Task LogUpdateAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T oldEntity, T newEntity, string? ipAddress = null, string? userAgent = null)
    {
        // Create simplified versions to avoid circular references
        var oldSafe = CreateSafeObject(oldEntity!);
        var newSafe = CreateSafeObject(newEntity!);
        
        await LogAsync(farmId, adminId, null, "UPDATE", entityType, entityId, oldSafe, newSafe, ipAddress, userAgent);
    }

    public async Task LogDeleteAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null)
    {
        await LogAsync(farmId, adminId, null, "DELETE", entityType, entityId, entity, null, ipAddress, userAgent);
    }

    public async Task LogSoftDeleteAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null)
    {
        await LogAsync(farmId, adminId, null, "SOFT_DELETE", entityType, entityId, entity, null, ipAddress, userAgent);
    }

    public async Task LogRestoreAsync<T>(int? farmId, int? adminId, string entityType, int entityId, T entity, string? ipAddress = null, string? userAgent = null)
    {
        await LogAsync(farmId, adminId, null, "RESTORE", entityType, entityId, null, entity, ipAddress, userAgent);
    }
}