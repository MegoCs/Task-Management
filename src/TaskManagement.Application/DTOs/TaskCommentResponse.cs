namespace TaskManagement.Application.DTOs;

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
