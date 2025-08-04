using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly MongoDbContext _context;

    public TaskRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Find(_ => true)
            .SortBy(t => t.Status)
            .ThenBy(t => t.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetTaskByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Find(t => t.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        // Set order to be at the end of the current status column
        var maxOrder = await _context.Tasks
            .Find(t => t.Status == task.Status)
            .SortByDescending(t => t.Order)
            .Project(t => t.Order)
            .FirstOrDefaultAsync(cancellationToken);

        task.Order = maxOrder + 1;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.Tasks.InsertOneAsync(task, cancellationToken: cancellationToken);
        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(string id, TaskItem task, CancellationToken cancellationToken = default)
    {
        task.UpdatedAt = DateTime.UtcNow;
        
        if (task.Status == DomainTaskStatus.Done && task.CompletedAt == null)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        var result = await _context.Tasks.ReplaceOneAsync(t => t.Id == id, task, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0 ? task : null;
    }

    public async Task<bool> DeleteTaskAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _context.Tasks.DeleteOneAsync(t => t.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<List<TaskItem>> GetTasksByStatusAsync(DomainTaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Find(t => t.Status == status)
            .SortBy(t => t.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TaskItem>> GetTasksByAssigneeAsync(string assigneeId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Find(t => t.AssigneeId == assigneeId)
            .SortBy(t => t.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateTaskOrderAsync(string id, int newOrder, DomainTaskStatus? newStatus = null, CancellationToken cancellationToken = default)
    {
        var update = Builders<TaskItem>.Update
            .Set(t => t.Order, newOrder)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        if (newStatus.HasValue)
        {
            update = update.Set(t => t.Status, newStatus.Value);
            
            if (newStatus.Value == DomainTaskStatus.Done)
            {
                update = update.Set(t => t.CompletedAt, DateTime.UtcNow);
            }
        }

        var result = await _context.Tasks.UpdateOneAsync(
            t => t.Id == id, 
            update, 
            cancellationToken: cancellationToken);
            
        return result.ModifiedCount > 0;
    }

    public async Task<List<TaskItem>> GetTasksDueSoonAsync(TimeSpan timespan, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Add(timespan);
        
        return await _context.Tasks
            .Find(t => t.DueDate.HasValue && 
                      t.DueDate.Value <= cutoffTime && 
                      t.DueDate.Value > DateTime.UtcNow &&
                      t.Status != DomainTaskStatus.Done)
            .ToListAsync(cancellationToken);
    }
}
