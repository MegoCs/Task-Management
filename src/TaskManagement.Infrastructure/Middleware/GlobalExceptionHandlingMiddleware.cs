using System.Net;
using System.Text.Json;
using TaskManagement.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware for centralized error processing
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundEx.Message;
                errorResponse.ErrorCode = "NOT_FOUND";
                _logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                break;

            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = validationEx.Message;
                errorResponse.ErrorCode = "VALIDATION_ERROR";
                errorResponse.Details = validationEx.Errors;
                _logger.LogWarning("Validation error: {Message}", validationEx.Message);
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorizedEx.Message;
                errorResponse.ErrorCode = "UNAUTHORIZED";
                _logger.LogWarning("Unauthorized access: {Message}", unauthorizedEx.Message);
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = conflictEx.Message;
                errorResponse.ErrorCode = "CONFLICT";
                _logger.LogWarning("Resource conflict: {Message}", conflictEx.Message);
                break;

            case TaskCanceledException:
            case OperationCanceledException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Message = "The request was cancelled";
                errorResponse.ErrorCode = "REQUEST_CANCELLED";
                _logger.LogInformation("Request was cancelled: {TraceId}", context.TraceIdentifier);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred";
                errorResponse.ErrorCode = "INTERNAL_ERROR";

                if (_environment.IsDevelopment())
                {
                    errorResponse.Details = new
                    {
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    };
                }

                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; }
}
