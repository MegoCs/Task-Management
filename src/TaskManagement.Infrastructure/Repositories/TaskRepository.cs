using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories.Interfaces;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly MongoDbContext _context;

    public TaskRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _context.Tasks
            .Find(_ => true)
            .SortBy(t => t.Order)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(string id)
    {
        return await _context.Tasks
            .Find(t => t.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        // Set order to be at the end of the current status column
        var maxOrder = await _context.Tasks
            .Find(t => t.Status == task.Status)
            .SortByDescending(t => t.Order)
            .Project(t => t.Order)
            .FirstOrDefaultAsync();

        task.Order = maxOrder + 1;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.Tasks.InsertOneAsync(task);
        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(string id, TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        
        if (task.Status == Domain.Entities.TaskStatus.Done && task.CompletedAt == null)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        var result = await _context.Tasks.ReplaceOneAsync(t => t.Id == id, task);
        return result.ModifiedCount > 0 ? task : null;
    }

    public async Task<bool> DeleteTaskAsync(string id)
    {
        var result = await _context.Tasks.DeleteOneAsync(t => t.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<TaskItem>> GetTasksByStatusAsync(Domain.Entities.TaskStatus status)
    {
        return await _context.Tasks
            .Find(t => t.Status == status)
            .SortBy(t => t.Order)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetTasksByAssigneeAsync(string assigneeId)
    {
        return await _context.Tasks
            .Find(t => t.AssigneeId == assigneeId)
            .SortBy(t => t.Order)
            .ToListAsync();
    }

    public async Task<bool> UpdateTaskOrderAsync(string id, int newOrder, Domain.Entities.TaskStatus? newStatus = null)
    {
        var update = Builders<TaskItem>.Update
            .Set(t => t.Order, newOrder)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        if (newStatus.HasValue)
        {
            update = update.Set(t => t.Status, newStatus.Value);
            
            if (newStatus.Value == Domain.Entities.TaskStatus.Done)
            {
                update = update.Set(t => t.CompletedAt, DateTime.UtcNow);
            }
        }

        var result = await _context.Tasks.UpdateOneAsync(t => t.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<List<TaskItem>> GetTasksDueSoonAsync(TimeSpan timespan)
    {
        var cutoffTime = DateTime.UtcNow.Add(timespan);
        
        return await _context.Tasks
            .Find(t => t.DueDate.HasValue && 
                      t.DueDate.Value <= cutoffTime && 
                      t.DueDate.Value > DateTime.UtcNow &&
                      t.Status != Domain.Entities.TaskStatus.Done)
            .ToListAsync();
    }
}
