using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Application.Validators;

namespace TaskManagement.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<ITaskService, TaskService>();
        
        // Add validation services
        services.AddValidationServices();
        
        return services;
    }
    
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Register validation service
        services.AddScoped<ValidationService>();
        
        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        
        return services;
    }
}
