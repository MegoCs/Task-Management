using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MassTransit;
using TaskManagement.API.Hubs;
using TaskManagement.API.Services;
using TaskManagement.Application.Extensions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Configuration;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure settings
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

// Configure SignalR with Redis backplane
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>() ?? new RedisSettings();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisSettings.ConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal(redisSettings.ChannelPrefix);
    });

// Add database context
builder.Services.AddSingleton<MongoDbContext>();

// Add repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Application layer services
builder.Services.AddApplicationServices();

// Add API layer services  
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() 
            ?? new RabbitMqSettings();

        cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
        {
            h.Username(rabbitMqSettings.Username);
            h.Password(rabbitMqSettings.Password);
        });
    });
});

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Configure SignalR authentication
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/taskHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
}

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TaskHub>("/taskHub");

app.Run();
