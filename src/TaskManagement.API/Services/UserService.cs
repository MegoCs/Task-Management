using System.Security.Claims;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUserDto? GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userId))
            return null;

        return new CurrentUserDto
        {
            Id = userId,
            Name = userName ?? string.Empty,
            Email = userEmail ?? string.Empty
        };
    }

    public string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetCurrentUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.Name)?.Value;
    }
}
