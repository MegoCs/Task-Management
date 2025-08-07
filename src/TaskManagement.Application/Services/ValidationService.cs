using FluentValidation;
using TaskManagement.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagement.Application.Services;

/// <summary>
/// Service for handling validation logic
/// </summary>
public class ValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Validates an object using FluentValidation and throws ValidationException if invalid
    /// </summary>
    public async Task ValidateAsync<T>(T request, CancellationToken cancellationToken = default) where T : class
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
            return; // No validator registered, skip validation

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            throw new Application.Exceptions.ValidationException(errors);
        }
    }

    /// <summary>
    /// Validates an object synchronously
    /// </summary>
    public void Validate<T>(T request) where T : class
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
            return; // No validator registered, skip validation

        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            throw new Application.Exceptions.ValidationException(errors);
        }
    }
}
