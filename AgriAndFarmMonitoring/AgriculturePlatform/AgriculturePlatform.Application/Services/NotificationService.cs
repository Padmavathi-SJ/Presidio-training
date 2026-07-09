// Application/Services/NotificationService.cs (using repository)
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task CreateNotificationAsync(int farmId, int? adminId, int? workerId, string title, string message, string type, string? actionUrl = null)
    {
        var notification = new Notification
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = workerId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            ActionUrl = actionUrl
        };

        await _notificationRepository.CreateAsync(notification);
    }

    public async Task CreateAlertAggregateNotificationAsync(int farmId, int? adminId, string title, string type, string listUrl, string? singleItemUrl = null)
    {
        var existing = await _notificationRepository.GetUnreadAggregateByTypeAsync(farmId, adminId, null, type);
        
        if (existing != null)
        {
            // Parse existing count from metadata or message. 
            // For simplicity, let's keep the count in Metadata or just increment a count we parse.
            int count = 1;
            if (existing.Metadata != null)
            {
                try {
                    var doc = System.Text.Json.JsonDocument.Parse(existing.Metadata.RootElement.GetRawText());
                    if (doc.RootElement.TryGetProperty("count", out var countElement))
                    {
                        count = countElement.GetInt32();
                    }
                } catch {}
            }
            count++;
            
            existing.Message = $"You have {count} unread {title.ToLower()} items.";
            existing.Title = $"{count} New {title}";
            existing.Metadata = System.Text.Json.JsonDocument.Parse($"{{\"count\": {count}}}");
            existing.ActionUrl = listUrl;
            existing.CreatedAt = DateTime.UtcNow; // Bump it to the top
            
            await _notificationRepository.UpdateAsync(existing);
        }
        else
        {
            var notification = new Notification
            {
                FarmId = farmId,
                AdminId = adminId,
                Title = $"1 New {title}",
                Message = $"You have 1 unread {title.ToLower()} item.",
                Type = type,
                IsRead = false,
                ActionUrl = singleItemUrl ?? listUrl,
                Metadata = System.Text.Json.JsonDocument.Parse($"{{\"count\": 1}}")
            };

            await _notificationRepository.CreateAsync(notification);
        }
    }
}