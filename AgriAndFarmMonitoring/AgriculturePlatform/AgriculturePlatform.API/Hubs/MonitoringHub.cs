// AgriculturePlatform.API/Hubs/MonitoringHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgriculturePlatform.API.Hubs;

public class MonitoringHub : Hub
{
    private static readonly Dictionary<string, UserConnection> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var farmId = Context.User?.FindFirst("farmId")?.Value;
        var role = Context.User?.FindFirst("role")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                FarmId = farmId,
                Role = role,
                ConnectedAt = DateTime.UtcNow
            };
        }

        if (!string.IsNullOrEmpty(farmId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"farm-{farmId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.Remove(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinFieldGroup(int fieldId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"field-{fieldId}");
    }

    public async Task LeaveFieldGroup(int fieldId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"field-{fieldId}");
    }

    public async Task JoinCropCycleGroup(int cropCycleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"cropcycle-{cropCycleId}");
    }

    public async Task AcknowledgeAlert(int alertId)
    {
        var userId = Context.UserIdentifier;
        await Clients.Group($"farm-{GetFarmId()}").SendAsync("AlertAcknowledged", 
            new { AlertId = alertId, AcknowledgedBy = userId, AcknowledgedAt = DateTime.UtcNow });
    }

    private string GetFarmId()
    {
        return Context.User?.FindFirst("farmId")?.Value ?? "0";
    }

    public async Task BroadcastSensorReading(object reading)
    {
        var farmId = GetFarmId();
        await Clients.Group($"farm-{farmId}").SendAsync("ReceiveSensorReading", reading);
    }

    public async Task BroadcastNewAlert(object alert)
    {
        var farmId = GetFarmId();
        await Clients.Group($"farm-{farmId}").SendAsync("NewAlert", alert);
    }

    public async Task BroadcastAlertResolved(int alertId)
    {
        var farmId = GetFarmId();
        await Clients.Group($"farm-{farmId}").SendAsync("AlertResolved", 
            new { AlertId = alertId, ResolvedAt = DateTime.UtcNow });
    }

    public async Task<int> GetConnectedUsersCount()
    {
        return await Task.FromResult(_userConnections.Count);
    }

    public async Task<object> GetConnectionStatus()
    {
        var userId = Context.UserIdentifier;
        var isConnected = _userConnections.ContainsKey(userId ?? "");
        
        // FIX: Use proper null handling
        UserConnection? connection = null;
        if (isConnected && userId != null)
        {
            _userConnections.TryGetValue(userId, out connection);
        }
        
        return await Task.FromResult(new
        {
            IsConnected = isConnected,
            ConnectionId = Context.ConnectionId,
            ConnectedAt = connection?.ConnectedAt
        });
    }
}

public class UserConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string? FarmId { get; set; }
    public string? Role { get; set; }
    public DateTime ConnectedAt { get; set; }
}