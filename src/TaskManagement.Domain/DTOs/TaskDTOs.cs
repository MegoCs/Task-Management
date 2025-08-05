namespace TaskManagement.Domain.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssigneeEmail { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; } = 1; // Medium
    public List<string> Tags { get; set; } = new();
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AssigneeEmail { get; set; }
    public DateTime? DueDate { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateTaskOrderRequest
{
    public string TaskId { get; set; } = string.Empty;
    public int NewOrder { get; set; }
    public int? NewStatus { get; set; }
}

public class TaskResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? AssigneeId { get; set; }
    public string? AssigneeEmail { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<TaskCommentResponse> Comments { get; set; } = new();
}

public class TaskCommentResponse
{
    public string Id { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ParentCommentId { get; set; }
    public List<TaskCommentResponse> Replies { get; set; } = new();
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
