using Microsoft.AspNetCore.SignalR;
using TaskManagement.API.Hubs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<TaskHub> _hubContext;

    public NotificationService(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTaskCreatedAsync(TaskResponse task)
    {
        await _hubContext.Clients.All.SendAsync("TaskCreated", task);
    }

    public async Task NotifyTaskUpdatedAsync(TaskResponse task)
    {
        await _hubContext.Clients.All.SendAsync("TaskUpdated", task);
    }

    public async Task NotifyTaskDeletedAsync(string taskId)
    {
        await _hubContext.Clients.All.SendAsync("TaskDeleted", taskId);
    }
}
