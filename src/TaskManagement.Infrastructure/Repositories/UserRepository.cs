using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<User?> UpdateUserAsync(string id, User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _context.Users.ReplaceOneAsync(u => u.Id == id, user, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0 ? user : null;
    }

    public async Task<bool> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _context.Users.DeleteOneAsync(u => u.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
