// API/Services/AlertNotificationService.cs
using Microsoft.AspNetCore.SignalR;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Hubs;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.API.Services;

public class AlertNotificationService : IAlertNotificationService
{
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly AgriculturePlatform.Application.Services.AlertNotificationService _appNotificationService;

    public AlertNotificationService(
        IHubContext<MonitoringHub> hubContext,
        AgriculturePlatform.Application.Services.AlertNotificationService appNotificationService)
    {
        _hubContext = hubContext;
        _appNotificationService = appNotificationService;
    }

    public async Task NotifyNewAlertAsync(int farmId, object alertData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("NewAlert", alertData);
        await _appNotificationService.NotifyNewAlertAsync(farmId, alertData);
    }

    public async Task NotifyAlertResolvedAsync(int farmId, object resolutionData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("AlertResolved", resolutionData);
        await _appNotificationService.NotifyAlertResolvedAsync(farmId, resolutionData);
    }

    public async Task NotifySensorReadingAsync(int farmId, object readingData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("ReceiveSensorReading", readingData);
        await _appNotificationService.NotifySensorReadingAsync(farmId, readingData);
    }

    public async Task SendAlertNotificationsAsync(Alert alert, int farmId)
    {
        // SignalR notification for real-time updates
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("NewAlert", new
        {
            alert.Id,
            alert.AlertType,
            alert.Severity,
            alert.Message,
            alert.FieldId,
            alert.CreatedAt
        });

        // Email notifications
        await _appNotificationService.SendAlertNotificationsAsync(alert, farmId);
    }

    public async Task SendTestAlertEmailAsync(string recipientEmail, string recipientName)
    {
        // For SignalR, just send a test notification
        await _hubContext.Clients.All.SendAsync("TestNotification", new
        {
            Message = $"Test alert sent to {recipientEmail}",
            Recipient = recipientName,
            Timestamp = DateTime.UtcNow
        });

        // Email notification
        await _appNotificationService.SendTestAlertEmailAsync(recipientEmail, recipientName);
    }
}