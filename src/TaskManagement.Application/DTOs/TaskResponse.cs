namespace TaskManagement.Application.DTOs;

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
