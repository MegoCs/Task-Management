using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace TaskManagement.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<DatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<TaskItem> Tasks => 
        _database.GetCollection<TaskItem>("tasks");

    public IMongoCollection<User> Users => 
        _database.GetCollection<User>("users");
}
