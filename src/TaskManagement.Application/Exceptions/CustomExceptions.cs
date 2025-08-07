namespace TaskManagement.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string resource, object key) 
        : base($"{resource} with key '{key}' was not found") { }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception thrown when user is not authorized
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Access denied") : base(message) { }
}

/// <summary>
/// Exception thrown when a resource conflict occurs
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
