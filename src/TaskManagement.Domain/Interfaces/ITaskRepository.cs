using TaskManagement.Domain.Entities;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateTaskAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateTaskAsync(string id, TaskItem task, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(string id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksByStatusAsync(DomainTaskStatus status, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksByAssigneeAsync(string assigneeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskOrderAsync(string id, int newOrder, DomainTaskStatus? newStatus = null, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksDueSoonAsync(TimeSpan timespan, CancellationToken cancellationToken = default);
}
