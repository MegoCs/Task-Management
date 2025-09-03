using TaskManagement.Infrastructure.Configuration;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Domain.Interfaces;
using TaskManagement.AuthService.Services;
using TaskManagement.AuthService.Extensions;
using TaskManagement.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure settings with validation
var databaseSettings = builder.Configuration.GetSection("Database").Get<DatabaseSettings>();
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

// Validate critical configuration
if (string.IsNullOrWhiteSpace(databaseSettings?.ConnectionString))
    throw new InvalidOperationException("Database connection string is required");
if (string.IsNullOrWhiteSpace(jwtSettings?.SecretKey))
    throw new InvalidOperationException("JWT secret key is required");

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Register services
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Add only validation services (not full application services since AuthService doesn't need TaskService)
builder.Services.AddValidationServices();

// Add CORS with more secure configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // In production, replace with actual frontend URLs
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "http://localhost:3000", "https://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Keep AllowAll for development only
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling middleware first
app.UseGlobalExceptionHandling();

// Add security headers
app.UseSecurityHeaders();

app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
