namespace TaskManagement.Application.DTOs;

public class UpdateTaskOrderRequest
{
    public string TaskId { get; set; } = string.Empty;
    public int NewOrder { get; set; }
    public int? NewStatus { get; set; }
}
