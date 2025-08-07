namespace TaskManagement.Application.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssigneeEmail { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; } = 1; // Medium
    public List<string> Tags { get; set; } = new();
}
