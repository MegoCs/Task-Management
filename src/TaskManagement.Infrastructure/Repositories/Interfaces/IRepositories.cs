using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(string id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(string id, TaskItem task);
    Task<bool> DeleteTaskAsync(string id);
    Task<List<TaskItem>> GetTasksByStatusAsync(Domain.Entities.TaskStatus status);
    Task<List<TaskItem>> GetTasksByAssigneeAsync(string assigneeId);
    Task<bool> UpdateTaskOrderAsync(string id, int newOrder, Domain.Entities.TaskStatus? newStatus = null);
    Task<List<TaskItem>> GetTasksDueSoonAsync(TimeSpan timespan);
}

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(string id, User user);
    Task<bool> DeleteUserAsync(string id);
}
