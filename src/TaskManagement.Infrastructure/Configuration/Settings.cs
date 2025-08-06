namespace TaskManagement.Infrastructure.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string TasksCollectionName { get; set; } = "tasks";
    public string UsersCollectionName { get; set; } = "users";
}

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Task Management System";
    public bool EnableSsl { get; set; } = true;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 1440; // 24 hours
}

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string ChannelPrefix { get; set; } = "TaskManagement";
}
