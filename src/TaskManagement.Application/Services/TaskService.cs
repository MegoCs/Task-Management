using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Messages;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

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

    public async Task<List<TaskResponse>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllTasksAsync(cancellationToken);
        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
        return task != null ? MapToResponse(task) : null;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string createdById, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            CreatedById = createdById,
            DueDate = request.DueDate,
            Priority = (TaskPriority)request.Priority,
            Tags = request.Tags,
            Status = DomainTaskStatus.Todo
        };

        // Set assignee if provided
        if (!string.IsNullOrEmpty(request.AssigneeEmail))
        {
            var assignee = await _userRepository.GetUserByEmailAsync(request.AssigneeEmail, cancellationToken);
            if (assignee != null)
            {
                task.AssigneeId = assignee.Id;
                task.AssigneeEmail = assignee.Email;
            }
        }

        var createdTask = await _taskRepository.CreateTaskAsync(task, cancellationToken);
        var response = MapToResponse(createdTask);

        // Publish task creation event
        await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
        {
            TaskId = createdTask.Id,
            Action = "Created",
            UserId = createdById,
            Data = response
        }, cancellationToken);

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
                }, cancellationToken);
            }
        }

        // Notify clients via SignalR
        await _notificationService.NotifyTaskCreatedAsync(response, cancellationToken);

        return response;
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string id, UpdateTaskRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var existingTask = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
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
            existingTask.Status = (DomainTaskStatus)request.Status.Value;

        if (request.Tags != null)
            existingTask.Tags = request.Tags;

        // Handle assignee change
        if (!string.IsNullOrEmpty(request.AssigneeEmail))
        {
            var assignee = await _userRepository.GetUserByEmailAsync(request.AssigneeEmail, cancellationToken);
            if (assignee != null)
            {
                existingTask.AssigneeId = assignee.Id;
                existingTask.AssigneeEmail = assignee.Email;
            }
        }

        var updatedTask = await _taskRepository.UpdateTaskAsync(id, existingTask, cancellationToken);
        if (updatedTask == null) return null;

        var response = MapToResponse(updatedTask);

        // Publish update event
        await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
        {
            TaskId = id,
            Action = "Updated",
            UserId = userId,
            Data = response
        }, cancellationToken);

        // Notify clients via SignalR
        await _notificationService.NotifyTaskUpdatedAsync(response, cancellationToken);

        return response;
    }

    public async Task<bool> DeleteTaskAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var success = await _taskRepository.DeleteTaskAsync(id, cancellationToken);
        
        if (success)
        {
            // Publish delete event
            await _messagePublisher.PublishTaskUpdateAsync(new TaskUpdateMessage
            {
                TaskId = id,
                Action = "Deleted",
                UserId = userId
            }, cancellationToken);

            // Notify clients via SignalR
            await _notificationService.NotifyTaskDeletedAsync(id, cancellationToken);
        }

        return success;
    }

    public async Task<bool> UpdateTaskOrderAsync(UpdateTaskOrderRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var success = await _taskRepository.UpdateTaskOrderAsync(
            request.TaskId, 
            request.NewOrder, 
            request.NewStatus.HasValue ? (DomainTaskStatus)request.NewStatus.Value : null,
            cancellationToken);

        if (success)
        {
            var updatedTask = await _taskRepository.GetTaskByIdAsync(request.TaskId, cancellationToken);
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
                }, cancellationToken);

                // Notify clients via SignalR
                await _notificationService.NotifyTaskUpdatedAsync(response, cancellationToken);
            }
        }

        return success;
    }

    public async Task<TaskResponse?> AddCommentAsync(string taskId, AddCommentRequest request, string userId, string userName, string userEmail, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, cancellationToken);
        if (task == null) return null;

        var comment = new TaskComment
        {
            AuthorId = userId,
            AuthorName = userName,
            AuthorEmail = userEmail,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            ParentCommentId = request.ParentCommentId
        };

        // If this is a reply to another comment
        if (!string.IsNullOrEmpty(request.ParentCommentId))
        {
            var parentComment = FindCommentById(task.Comments, request.ParentCommentId);
            if (parentComment == null) return null; // Parent comment not found

            parentComment.Replies.Add(comment);
        }
        else
        {
            // This is a top-level comment
            task.Comments.Add(comment);
        }

        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateTaskAsync(taskId, task, cancellationToken);
        if (updatedTask == null) return null;

        var response = MapToResponse(updatedTask);

        // Notify clients via SignalR
        await _notificationService.NotifyTaskUpdatedAsync(response, cancellationToken);

        return response;
    }

    private static TaskComment? FindCommentById(List<TaskComment> comments, string commentId)
    {
        foreach (var comment in comments)
        {
            if (comment.Id == commentId)
                return comment;

            var reply = FindCommentById(comment.Replies, commentId);
            if (reply != null)
                return reply;
        }
        return null;
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
            Comments = task.Comments.Select(MapCommentToResponse).ToList()
        };
    }

    private static TaskCommentResponse MapCommentToResponse(TaskComment comment)
    {
        return new TaskCommentResponse
        {
            Id = comment.Id,
            AuthorId = comment.AuthorId,
            AuthorName = comment.AuthorName,
            AuthorEmail = comment.AuthorEmail,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies.Select(MapCommentToResponse).ToList()
        };
    }
}
