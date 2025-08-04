using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllTasksAsync();
    Task<TaskResponse?> GetTaskByIdAsync(string id);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string createdById);
    Task<TaskResponse?> UpdateTaskAsync(string id, UpdateTaskRequest request, string userId);
    Task<bool> DeleteTaskAsync(string id, string userId);
    Task<bool> UpdateTaskOrderAsync(UpdateTaskOrderRequest request, string userId);
    Task<TaskResponse?> AddCommentAsync(string taskId, AddCommentRequest request, string userId, string userName);
}
