using Microsoft.AspNetCore.Mvc;
using TaskManagement.AuthService.Services;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Repositories.Interfaces;
using BCrypt.Net;

namespace TaskManagement.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository, 
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name, email, and password are required");
            }

            // Check if user already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt failed - user already exists: {Email}", request.Email);
                return BadRequest("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for email: {Email}", request.Email);
            return StatusCode(500, "An error occurred during registration");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to login user with email: {Email}", request.Email);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required");
            }

            var user = await _userRepository.GetUserByEmailAsync(request.Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed - user not found: {Email}", request.Email);
                return BadRequest("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed - invalid password for user: {Email}", request.Email);
                return BadRequest("Invalid email or password");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login for email: {Email}", request.Email);
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new { Status = "Healthy", Service = "AuthService", Timestamp = DateTime.UtcNow });
    }
}
