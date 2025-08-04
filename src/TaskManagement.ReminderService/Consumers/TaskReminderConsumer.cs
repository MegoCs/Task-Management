using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Messages;
using TaskManagement.Application.Interfaces;

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
                    message.DueDate,
                    context.CancellationToken);

                _logger.LogInformation("Reminder sent for task {TaskId} to {Email}", 
                    message.TaskId, message.AssigneeEmail);
            }
            else
            {
                // If reminder is not due yet, just log and skip
                // In a production system, you might want to use a proper scheduler or delay queue
                var delay = message.ScheduledFor - DateTime.UtcNow;
                _logger.LogInformation("Reminder for task {TaskId} scheduled for {ScheduledTime}, not due yet (in {Delay})", 
                    message.TaskId, message.ScheduledFor, delay);
                
                // For now, we'll just return without processing
                // In a more sophisticated setup, you could:
                // 1. Use Quartz.NET for scheduling
                // 2. Use Azure Service Bus scheduled messages
                // 3. Use RabbitMQ delayed message plugin
                // 4. Requeue with a delay using proper scheduler setup
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder message for task {TaskId}", message.TaskId);
            throw; // This will cause the message to be retried or moved to error queue
        }
    }
}
