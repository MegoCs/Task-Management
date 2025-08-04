namespace TaskManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendTaskReminderAsync(string toEmail, string toName, string taskTitle, DateTime dueDate, CancellationToken cancellationToken = default);
    Task SendTaskAssignmentAsync(string toEmail, string toName, string taskTitle, string assignedBy, CancellationToken cancellationToken = default);
}
