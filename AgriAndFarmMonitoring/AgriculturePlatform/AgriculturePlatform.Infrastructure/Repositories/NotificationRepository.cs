// Infrastructure/Repositories/NotificationRepository.cs
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(int farmId, int? adminId, int? workerId)
    {
        var query = _context.Notifications
            .Where(n => n.FarmId == farmId && !n.IsRead);

        if (adminId.HasValue)
            query = query.Where(n => n.AdminId == adminId);
        if (workerId.HasValue)
            query = query.Where(n => n.WorkerId == workerId);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}