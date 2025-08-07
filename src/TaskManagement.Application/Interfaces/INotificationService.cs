using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface INotificationService
{
    Task NotifyTaskCreatedAsync(TaskResponse task, CancellationToken cancellationToken = default);
    Task NotifyTaskUpdatedAsync(TaskResponse task, CancellationToken cancellationToken = default);
    Task NotifyTaskDeletedAsync(string taskId, CancellationToken cancellationToken = default);
    Task NotifyTaskAssignedAsync(TaskResponse task, CancellationToken cancellationToken = default);
}
