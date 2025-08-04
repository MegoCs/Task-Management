using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Repositories.Interfaces;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationService _notificationService;

    public TaskService(
        ITaskRepository taskRepository, 
        IUserRepository userRepository,
        IMessagePublisher messagePublisher,
        INotificationService notificationService)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _messagePublisher = messagePublisher;
        _notificationService = notificationService;
    }

    public async Task<List<TaskResponse>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllTasksAsync();
        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(string id)
    {
        var task = await _taskRepository.GetTaskByIdAsync(id);
        return task != null ? MapToResponse(task) : null;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string createdById)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            CreatedById = createdById,
            DueDate = request.DueDate,
            Priority = (TaskPriority)request.Priority,
            Tags = request.Tags,
            Status = Domain.Entities.TaskStatus.Todo
        };

        // Set assignee if provided
        if (!string.IsNullOrEmpty(request.AssigneeEmail))
        {
            var assignee = await _userRepository.GetUserByEmailAsync(request.AssigneeEmail);
            if (assignee != null)
            {
                task.AssigneeId = assignee.Id;
                task.AssigneeEmail = assignee.Email;
            }
        }

        var createdTask = await _taskRepository.CreateTaskAsync(task);
        var response = MapToResponse(createdTask);

        // Publish task creation event
        await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
        {
            TaskId = createdTask.Id,
            Action = "Created",
            UserId = createdById,
            Data = response
        });

        // Schedule reminder if due date is set
        if (createdTask.DueDate.HasValue && !string.IsNullOrEmpty(createdTask.AssigneeEmail))
        {
             var reminderTime = createdTask.DueDate.Value.AddHours(-24);
             if (reminderTime > DateTime.UtcNow)
            {
                await _messagePublisher.PublishTaskReminderAsync(new TaskReminderMessage
                {
                    TaskId = createdTask.Id,
                    TaskTitle = createdTask.Title,
                    AssigneeEmail = createdTask.AssigneeEmail,
                    AssigneeName = createdTask.AssigneeEmail, // Could be improved with actual name
                    DueDate = createdTask.DueDate.Value,
                    ScheduledFor = reminderTime
                });
            }
        }

        // Notify clients via SignalR
        await _notificationService.NotifyTaskCreatedAsync(response);

        return response;
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string id, UpdateTaskRequest request, string userId)
    {
        var existingTask = await _taskRepository.GetTaskByIdAsync(id);
        if (existingTask == null) return null;

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Title))
            existingTask.Title = request.Title;
        
        if (!string.IsNullOrEmpty(request.Description))
            existingTask.Description = request.Description;

        if (request.DueDate.HasValue)
            existingTask.DueDate = request.DueDate;

        if (request.Priority.HasValue)
            existingTask.Priority = (TaskPriority)request.Priority.Value;

        if (request.Status.HasValue)
            existingTask.Status = (Domain.Entities.TaskStatus)request.Status.Value;

        if (request.Tags != null)
            existingTask.Tags = request.Tags;

        // Handle assignee change
        if (!string.IsNullOrEmpty(request.AssigneeEmail))
        {
            var assignee = await _userRepository.GetUserByEmailAsync(request.AssigneeEmail);
            if (assignee != null)
            {
                existingTask.AssigneeId = assignee.Id;
                existingTask.AssigneeEmail = assignee.Email;
            }
        }

        var updatedTask = await _taskRepository.UpdateTaskAsync(id, existingTask);
        if (updatedTask == null) return null;

        var response = MapToResponse(updatedTask);

        // Publish update event
        await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
        {
            TaskId = id,
            Action = "Updated",
            UserId = userId,
            Data = response
        });

        // Notify clients via SignalR
        await _notificationService.NotifyTaskUpdatedAsync(response);

        return response;
    }

    public async Task<bool> DeleteTaskAsync(string id, string userId)
    {
        var success = await _taskRepository.DeleteTaskAsync(id);
        
        if (success)
        {
            // Publish delete event
            await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
            {
                TaskId = id,
                Action = "Deleted",
                UserId = userId
            });

            // Notify clients via SignalR
            await _notificationService.NotifyTaskDeletedAsync(id);
        }

        return success;
    }

    public async Task<bool> UpdateTaskOrderAsync(UpdateTaskOrderRequest request, string userId)
    {
        var success = await _taskRepository.UpdateTaskOrderAsync(
            request.TaskId, 
            request.NewOrder, 
            request.NewStatus.HasValue ? (Domain.Entities.TaskStatus)request.NewStatus.Value : null);

        if (success)
        {
            var updatedTask = await _taskRepository.GetTaskByIdAsync(request.TaskId);
            if (updatedTask != null)
            {
                var response = MapToResponse(updatedTask);

                // Publish update event
                await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
                {
                    TaskId = request.TaskId,
                    Action = "OrderChanged",
                    UserId = userId,
                    Data = response
                });

                // Notify clients via SignalR
                await _notificationService.NotifyTaskUpdatedAsync(response);
            }
        }

        return success;
    }

    public async Task<TaskResponse?> AddCommentAsync(string taskId, AddCommentRequest request, string userId, string userName)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId);
        if (task == null) return null;

        var comment = new TaskComment
        {
            AuthorId = userId,
            AuthorName = userName,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        task.Comments.Add(comment);
        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateTaskAsync(taskId, task);
        if (updatedTask == null) return null;

        var response = MapToResponse(updatedTask);

        // Notify clients via SignalR
        await _notificationService.NotifyTaskUpdatedAsync(response);

        return response;
    }

    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = (int)task.Status,
            StatusName = task.Status.ToString(),
            Priority = (int)task.Priority,
            PriorityName = task.Priority.ToString(),
            AssigneeId = task.AssigneeId,
            AssigneeEmail = task.AssigneeEmail,
            CreatedById = task.CreatedById,
            DueDate = task.DueDate,
            Order = task.Order,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            Tags = task.Tags,
            Comments = task.Comments.Select(c => new TaskCommentResponse
            {
                Id = c.Id,
                AuthorId = c.AuthorId,
                AuthorName = c.AuthorName,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }
}
