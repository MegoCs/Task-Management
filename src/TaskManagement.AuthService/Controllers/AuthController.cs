using Microsoft.AspNetCore.Mvc;
using TaskManagement.AuthService.Services;
using TaskManagement.Application.Services;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Application.Exceptions;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace TaskManagement.AuthService.Controllers;

/// <summary>
/// Authentication controller providing user registration and login endpoints
/// </summary>
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

    /// <summary>
    /// Registers a new user with enhanced input validation and sanitization
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        await _validationService.ValidateAsync(request, cancellationToken);

        // Sanitize inputs to prevent XSS
        var sanitizedName = SanitizeInput(request.Name);
        var sanitizedEmail = SanitizeInput(request.Email);
        
        var normalizedEmail = sanitizedEmail.Trim().ToLowerInvariant();

        // Check if user already exists
        var existingUser = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists");

        // Create new user
        var user = new User
        {
            Name = sanitizedName.Trim(),
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

    /// <summary>
    /// Authenticates a user with enhanced security measures
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to login user with email: {Email}", request.Email);

        await _validationService.ValidateAsync(request, cancellationToken);

        // Sanitize input to prevent XSS
        var sanitizedEmail = SanitizeInput(request.Email);
        var normalizedEmail = sanitizedEmail.Trim().ToLowerInvariant();
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

    /// <summary>
    /// Health check endpoint for service monitoring
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new { Status = "Healthy", Service = "AuthService", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Sanitizes input string to prevent XSS attacks
    /// </summary>
    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
            
        // Remove potentially harmful characters and HTML tags
        var sanitized = Regex.Replace(input, @"<[^>]*>", string.Empty);
        sanitized = Regex.Replace(sanitized, @"[<>""']", string.Empty);
        
        return sanitized.Trim();
    }
}
