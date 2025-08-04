using MassTransit;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishTaskReminderAsync(TaskReminderMessage message, CancellationToken cancellationToken = default);
    Task PublishTaskUpdateAsync(TaskUpdateMessage message, CancellationToken cancellationToken = default);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishTaskReminderAsync(TaskReminderMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation("Published task reminder for task {TaskId}", message.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task reminder for task {TaskId}", message.TaskId);
            throw;
        }
    }

    public async Task PublishTaskUpdateAsync(TaskUpdateMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation("Published task update for task {TaskId}", message.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task update for task {TaskId}", message.TaskId);
            throw;
        }
    }
}
