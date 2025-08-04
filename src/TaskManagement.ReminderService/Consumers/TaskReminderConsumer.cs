using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.ReminderService.Consumers;

public class TaskReminderConsumer : IConsumer<TaskReminderMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TaskReminderConsumer> _logger;

    public TaskReminderConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<TaskReminderConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TaskReminderMessage> context)
    {
        var message = context.Message;
        
        try
        {
            _logger.LogInformation("Processing reminder for task {TaskId}", message.TaskId);

            // Check if it's time to send the reminder
            if (DateTime.UtcNow >= message.ScheduledFor)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendTaskReminderAsync(
                    message.AssigneeEmail,
                    message.AssigneeName,
                    message.TaskTitle,
                    message.DueDate);

                _logger.LogInformation("Reminder sent for task {TaskId} to {Email}", 
                    message.TaskId, message.AssigneeEmail);
            }
            else
            {
                // Schedule for later processing by redelivering the message with a delay
                var delay = message.ScheduledFor - DateTime.UtcNow;
                _logger.LogInformation("Reminder for task {TaskId} scheduled for {ScheduledTime}, delaying for {Delay}", 
                    message.TaskId, message.ScheduledFor, delay);

                // Reschedule the message
                await context.ScheduleSend(delay, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder message for task {TaskId}", message.TaskId);
            throw; // This will cause the message to be retried or moved to error queue
        }
    }
}
