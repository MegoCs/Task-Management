using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.API.Hubs;

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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any groups if needed
        await base.OnDisconnectedAsync(exception);
    }
}
