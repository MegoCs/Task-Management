using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories.Interfaces;

namespace TaskManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public async Task<User?> UpdateUserAsync(string id, User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
        return result.ModifiedCount > 0 ? user : null;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}
