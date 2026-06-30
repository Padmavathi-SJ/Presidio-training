// AgriculturePlatform.API/Hubs/WeatherHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgriculturePlatform.API.Hubs;

[Authorize]
public class WeatherHub : Hub
{
    private readonly ILogger<WeatherHub> _logger;

    public WeatherHub(ILogger<WeatherHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var farmId = Context.User?.FindFirst("farmId")?.Value;
        var userType = Context.User?.FindFirst("userType")?.Value;

        _logger.LogInformation($"User {userId} connected to WeatherHub. Farm: {farmId}, Type: {userType}");

        // Join admin group if user is admin
        if (userType == "Admin" && !string.IsNullOrEmpty(farmId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"admin_{farmId}");
            _logger.LogInformation($"Admin {userId} joined admin group for farm {farmId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"User {userId} disconnected from WeatherHub");
        await base.OnDisconnectedAsync(exception);
    }

    // Client methods
    public async Task JoinAdminGroup(int farmId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"admin_{farmId}");
        _logger.LogInformation($"User joined admin group for farm {farmId}");
    }

    public async Task LeaveAdminGroup(int farmId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"admin_{farmId}");
        _logger.LogInformation($"User left admin group for farm {farmId}");
    }

    public async Task SubscribeToField(int fieldId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"field_{fieldId}");
        _logger.LogInformation($"User subscribed to field {fieldId}");
    }

    public async Task UnsubscribeFromField(int fieldId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"field_{fieldId}");
        _logger.LogInformation($"User unsubscribed from field {fieldId}");
    }

    // Server methods to send updates
    public async Task SendWeatherUpdate(int farmId, object weatherData)
    {
        await Clients.Group($"admin_{farmId}").SendAsync("WeatherUpdated", weatherData);
    }

    public async Task SendAlertCreated(int farmId, object alert)
    {
        await Clients.Group($"admin_{farmId}").SendAsync("AlertCreated", alert);
    }

    public async Task SendAlertAcknowledged(int farmId, int alertId)
    {
        await Clients.Group($"admin_{farmId}").SendAsync("AlertAcknowledged", alertId);
    }

    public async Task SendAlertCountUpdated(int farmId, int count)
    {
        await Clients.Group($"admin_{farmId}").SendAsync("AlertCountUpdated", count);
    }
}