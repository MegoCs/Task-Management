namespace TaskManagement.Domain.Messages;

public record TaskCreatedMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string Action { get; init; } = "Created";
    public string UserId { get; init; } = string.Empty;
    public object? Data { get; init; } // Changed from TaskResponse to object to remove DTO dependency
}

public record TaskUpdateMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty; // Created, Updated, Deleted, StatusChanged
    public string UserId { get; init; } = string.Empty;
    public object? Data { get; init; }
}

public record TaskDeletedMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string Action { get; init; } = "Deleted";
    public string UserId { get; init; } = string.Empty;
}

public record TaskReminderMessage
{
    public string TaskId { get; init; } = string.Empty;
    public string TaskTitle { get; init; } = string.Empty;
    public string AssigneeEmail { get; init; } = string.Empty;
    public string AssigneeName { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public DateTime ScheduledFor { get; init; }
}
