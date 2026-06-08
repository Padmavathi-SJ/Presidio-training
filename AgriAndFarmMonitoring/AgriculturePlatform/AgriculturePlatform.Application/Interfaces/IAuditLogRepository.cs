
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task SaveChangesAsync();
}