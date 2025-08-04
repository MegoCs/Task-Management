using TaskManagement.Domain.Messages;

namespace TaskManagement.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishTaskCreatedAsync(TaskCreatedMessage message, CancellationToken cancellationToken = default);
    Task PublishTaskUpdateAsync(TaskUpdateMessage message, CancellationToken cancellationToken = default);
    Task PublishTaskDeletedAsync(TaskDeletedMessage message, CancellationToken cancellationToken = default);
    Task PublishTaskReminderAsync(TaskReminderMessage message, CancellationToken cancellationToken = default);
}
