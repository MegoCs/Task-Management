using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TaskManagement.API.Hubs;

[Authorize]
public class TaskHub : Hub
{
    public async Task JoinGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
