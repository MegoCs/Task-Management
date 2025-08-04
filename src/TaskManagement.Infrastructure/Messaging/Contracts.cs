namespace TaskManagement.Infrastructure.Messaging;

public record TaskReminderMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string TaskTitle { get; init; } = string.Empty;
    public string AssigneeEmail { get; init; } = string.Empty;
    public string AssigneeName { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public DateTime ScheduledFor { get; init; }
}

public record TaskUpdateMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty; // Created, Updated, Deleted, StatusChanged
    public string UserId { get; init; } = string.Empty;
    public object? Data { get; init; }
}
