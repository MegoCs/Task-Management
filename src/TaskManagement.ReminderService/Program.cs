using MassTransit;
using TaskManagement.Infrastructure.Configuration;
using TaskManagement.Infrastructure.Services;
using TaskManagement.ReminderService.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Configure settings
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// Add services
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TaskReminderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() 
            ?? new RabbitMqSettings();

        cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
        {
            h.Username(rabbitMqSettings.Username);
            h.Password(rabbitMqSettings.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
