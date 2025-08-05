using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string createdById, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateTaskAsync(string id, UpdateTaskRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskOrderAsync(UpdateTaskOrderRequest request, string userId, CancellationToken cancellationToken = default);
    Task<TaskResponse?> AddCommentAsync(string taskId, AddCommentRequest request, string userId, string userName, string userEmail, CancellationToken cancellationToken = default);
}
