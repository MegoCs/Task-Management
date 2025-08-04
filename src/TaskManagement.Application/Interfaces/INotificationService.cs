using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface INotificationService
{
    Task NotifyTaskCreatedAsync(TaskResponse task);
    Task NotifyTaskUpdatedAsync(TaskResponse task);
    Task NotifyTaskDeletedAsync(string taskId);
}
