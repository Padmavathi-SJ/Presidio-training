// API/Services/NotificationService.cs
using AgriculturePlatform.API.Hubs;
using AgriculturePlatform.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AgriculturePlatform.API.Services;

public class NotificationService : INotificationService
{
    private readonly AgriculturePlatform.Application.Services.NotificationService _appService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        AgriculturePlatform.Application.Services.NotificationService appService,
        IHubContext<NotificationHub> hubContext)
    {
        _appService = appService;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(int farmId, int? adminId, int? workerId, string title, string message, string type, string? actionUrl = null)
    {
        await _appService.CreateNotificationAsync(farmId, adminId, workerId, title, message, type, actionUrl);

        var payload = new { Title = title, Message = message, Type = type, ActionUrl = actionUrl, CreatedAt = DateTime.UtcNow };

        if (adminId.HasValue)
        {
            await _hubContext.Clients.Group($"Farm-{farmId}-Admin").SendAsync("ReceiveNotification", payload);
        }
        else if (workerId.HasValue)
        {
            await _hubContext.Clients.Group($"Worker-{workerId}").SendAsync("ReceiveNotification", payload);
        }
    }

    public async Task CreateAlertAggregateNotificationAsync(int farmId, int? adminId, string title, string type, string listUrl, string? singleItemUrl = null)
    {
        await _appService.CreateAlertAggregateNotificationAsync(farmId, adminId, title, type, listUrl, singleItemUrl);

        var payload = new { Title = title, Message = "You have new unread alerts.", Type = type, ActionUrl = listUrl, CreatedAt = DateTime.UtcNow };

        if (adminId.HasValue)
        {
            await _hubContext.Clients.Group($"Farm-{farmId}-Admin").SendAsync("ReceiveNotification", payload);
        }
    }
}
