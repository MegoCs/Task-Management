using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Configuration;

namespace TaskManagement.AuthService.Services;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <returns>A JWT token string</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="ArgumentException">Thrown when required user properties are missing</exception>
    /// <exception cref="InvalidOperationException">Thrown when token generation fails</exception>
    string GenerateToken(User user);
}

/// <summary>
/// JWT token service implementation with enhanced security and validation
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the JwtService with validated settings
    /// </summary>
    /// <param name="jwtSettings">JWT configuration settings</param>
    /// <exception cref="ArgumentNullException">Thrown when jwtSettings is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when required settings are missing or invalid</exception>
    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        
        // Validate required JWT settings
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is required");
        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
            throw new InvalidOperationException("JWT Issuer is required");
        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            throw new InvalidOperationException("JWT Audience is required");
        if (_jwtSettings.ExpirationMinutes <= 0)
            throw new InvalidOperationException("JWT ExpirationMinutes must be greater than 0");
    }

    /// <summary>
    /// Generates a secure JWT token for the specified user with enhanced validation
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <returns>A signed JWT token string</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="ArgumentException">Thrown when required user properties are missing</exception>
    /// <exception cref="InvalidOperationException">Thrown when token generation fails</exception>
    public string GenerateToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrWhiteSpace(user.Id))
            throw new ArgumentException("User ID is required", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("User email is required", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ArgumentException("User name is required", nameof(user));

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("jti", Guid.NewGuid().ToString()), // JWT ID for token uniqueness
                    new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate JWT token", ex);
        }
    }
}
