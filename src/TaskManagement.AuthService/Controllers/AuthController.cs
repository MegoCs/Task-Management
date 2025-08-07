using Microsoft.AspNetCore.Mvc;
using TaskManagement.AuthService.Services;
using TaskManagement.Application.Services;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Exceptions;
using BCrypt.Net;

namespace TaskManagement.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ValidationService _validationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository, 
        IJwtService jwtService,
        ValidationService validationService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        await _validationService.ValidateAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if user already exists
        var existingUser = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists");

        // Create new user
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var createdUser = await _userRepository.CreateUserAsync(user, cancellationToken);
        var token = _jwtService.GenerateToken(createdUser);

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to login user with email: {Email}", request.Email);

        await _validationService.ValidateAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        
        // Use a generic message for security (don't reveal whether user exists)
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed for email: {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new { Status = "Healthy", Service = "AuthService", Timestamp = DateTime.UtcNow });
    }
}
