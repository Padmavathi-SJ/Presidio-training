// AgriculturePlatform.API/Hubs/SensorHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace AgriculturePlatform.API.Hubs;

[Authorize]
public class SensorHub : Hub
{
    private static readonly Dictionary<string, string> _userConnections = new();
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var farmId = Context.User?.FindFirst("farmId")?.Value;
        
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

    public async Task JoinFarmGroup(int farmId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"farm-{farmId}");
    }

    public async Task LeaveFarmGroup(int farmId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"farm-{farmId}");
    }
}