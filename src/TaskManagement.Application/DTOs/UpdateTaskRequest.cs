namespace TaskManagement.Application.DTOs;

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
