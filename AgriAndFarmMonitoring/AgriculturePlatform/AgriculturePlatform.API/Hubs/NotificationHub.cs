using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgriculturePlatform.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var farmIdStr = Context.User?.FindFirst("FarmId")?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(farmIdStr) && !string.IsNullOrEmpty(role))
        {
            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Farm-{farmIdStr}-Admin");
            }
            else if (role == "Worker" && !string.IsNullOrEmpty(userIdStr))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Worker-{userIdStr}");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var farmIdStr = Context.User?.FindFirst("FarmId")?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(farmIdStr) && !string.IsNullOrEmpty(role))
        {
            if (role == "Admin")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Farm-{farmIdStr}-Admin");
            }
            else if (role == "Worker" && !string.IsNullOrEmpty(userIdStr))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Worker-{userIdStr}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
