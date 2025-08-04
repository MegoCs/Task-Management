using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IUserService
{
    CurrentUserDto? GetCurrentUser();
    string? GetCurrentUserId();
    string? GetCurrentUserName();
}
