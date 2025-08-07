namespace TaskManagement.Application.DTOs;

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
}
