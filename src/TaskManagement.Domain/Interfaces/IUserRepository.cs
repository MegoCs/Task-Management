using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> UpdateUserAsync(string id, User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string id, CancellationToken cancellationToken = default);
}
