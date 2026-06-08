// AgriculturePlatform.Infrastructure/Repositories/AuditLogRepository.cs
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}