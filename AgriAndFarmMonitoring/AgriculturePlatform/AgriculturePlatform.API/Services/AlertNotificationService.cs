// AgriculturePlatform.API/Services/AlertNotificationService.cs
using Microsoft.AspNetCore.SignalR;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.API.Hubs;

namespace AgriculturePlatform.API.Services;

public class AlertNotificationService : IAlertNotificationService
{
    private readonly IHubContext<MonitoringHub> _hubContext;

    public AlertNotificationService(IHubContext<MonitoringHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewAlertAsync(int farmId, object alertData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("NewAlert", alertData);
    }

    public async Task NotifyAlertResolvedAsync(int farmId, object resolutionData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("AlertResolved", resolutionData);
    }

    public async Task NotifySensorReadingAsync(int farmId, object readingData)
    {
        await _hubContext.Clients.Group($"farm-{farmId}").SendAsync("ReceiveSensorReading", readingData);
    }
}