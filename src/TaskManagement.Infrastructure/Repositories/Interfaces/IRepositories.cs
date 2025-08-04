using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateTaskAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateTaskAsync(string id, TaskItem task, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(string id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksByStatusAsync(Domain.Entities.TaskStatus status, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksByAssigneeAsync(string assigneeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskOrderAsync(string id, int newOrder, Domain.Entities.TaskStatus? newStatus = null, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksDueSoonAsync(TimeSpan timespan, CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> UpdateUserAsync(string id, User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string id, CancellationToken cancellationToken = default);
}
